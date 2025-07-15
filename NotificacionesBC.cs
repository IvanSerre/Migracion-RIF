using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.Math;
using DocumentFormat.OpenXml.Wordprocessing;
using Newtonsoft.Json;
using RIF_Logic_Access.BE;
using RIF_Logic_Access.DAC;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Web;

namespace RIF_Logic_Access.BC
{
    public class NotificacionesBC
    {
        protected UsuarioBE usuarioLog = new UsuarioBE();

        public NotificacionesBC(UsuarioBE objUsu)
        {
            usuarioLog = objUsu;
        }
        public DataTable RescataListadoCatalogoNotificaciones()
        {
            try
            {
                DataTable dtNot = new NotificacionesDAC(usuarioLog).RescataCatalogoNotificaciones(null);

                DataRow fila = dtNot.NewRow();
                fila["cnonomcno_ds"] = "[Selecciona Tipo de Notificación]";
                fila["cnocodcno_id"] = "0";

                dtNot.Rows.InsertAt(fila, 0);
                return dtNot;
            }
            catch (Exception ex)
            {
                LogLocalBE log = new LogLocalBE()
                {
                    Fecha = DateTime.Now,
                    Funcion = "NotificacionesBC",
                    Mensaje = ex.Message,
                    Metodo = "RescataListadoCatalogoNotificaciones",
                    Parametros = "{}",
                    Usuario = usuarioLog.idUsuario.ToString()
                };

                new LogSessionesDAC(usuarioLog).GrabaLogLocalErrores(log);
                throw new Exception(ex.Message);
            }
        }
        public DataTable RescataCatalogoNotificaciones(string codCatNotificacion)
        {
            try
            {
                return new NotificacionesDAC(usuarioLog).RescataCatalogoNotificaciones(codCatNotificacion);
            }
            catch (Exception ex)
            {
                LogLocalBE log = new LogLocalBE()
                {
                    Fecha = DateTime.Now,
                    Funcion = "NotificacionesBC",
                    Mensaje = ex.Message,
                    Metodo = "RescataCatalogoNotificaciones",
                    Parametros = "{codCatNotificacion:" + codCatNotificacion + "}",
                    Usuario = usuarioLog.idUsuario.ToString()
                };

                new LogSessionesDAC(usuarioLog).GrabaLogLocalErrores(log);
                throw new Exception(ex.Message);
            }
        }
        public NotificacionBE SetNotificacion(NotificacionBE Notificacion, bool Transaccional, PersonaLteBE Persona)
        {
            PersonaNotificacionBE PersonaNot = new PersonaNotificacionBE();
            try
            {
                if (Notificacion.CodEntidad == 0) throw new Exception("Entidad no indicada para el ingreso de la notificación");
                if (Notificacion.UsuarioCreador == 0) throw new Exception("Usuario creador no indicado");
                if (string.IsNullOrEmpty(Notificacion.NombreNotificacion)) throw new Exception("Debes indicar el nombre de la notificación");
                if (Notificacion.FechaNot == DateTime.MinValue) throw new Exception("Debes indicar la fecha de envío de la notificación");
                if (string.IsNullOrEmpty(Notificacion.TipoNot)) throw new Exception("Debes indicar el tipo de la notificación");

                if (Notificacion.TipoNot == "SODO" && (Notificacion.Antecedentes == null || Notificacion.Antecedentes.Count == 0))
                    throw new Exception("Al ingresar una notificacion de solicitud de documentos debes agregar los documentos que se deben enviar");
                if (Notificacion.TipoNot == "SOFI" && (Notificacion.DocumentoAdjunto == null || Notificacion.DocumentoAdjunto.Documento.Length == 0))
                    throw new Exception("Al ingresar una notificacion de solicitud de firma debes agregar el documento que se firmará");
                if (Notificacion.TipoNot == "SOFI" && (Notificacion.DocumentoAdjunto?.TipoDoc == null || Notificacion.DocumentoAdjunto?.TipoDoc.Length == 0))
                    throw new Exception("Al ingresar una notificacion de solicitud de firma debes agregar el tipo de documento que se firmará");

                Notificacion = new NotificacionesDAC(usuarioLog).SetNotificacion(Notificacion);
                if (Notificacion.CodNotificacion != 0)
                {
                    foreach (DestinatariosBE destinatario in Notificacion.Destinatarios)
                    {
                        destinatario.CodNotificacion = Notificacion.CodNotificacion;
                        InsertDestinatarioNoficiacion(destinatario);

                        if (Transaccional)
                        {
                            try
                            {
                                using (DataTable persona = new PersonaBC(usuarioLog).RescataPersona(Notificacion.CodEntidad, null, destinatario.CodDestinatario))
                                {
                                    if (persona.Rows.Count > 0)
                                    {
                                        ResponseSendMail Response_message = new ResponseSendMail();
                                        switch (Notificacion.TipoNot)
                                        {
                                            case "SODO":
                                                Response_message = new CorreosBC(usuarioLog).EnviaCorreoApi(new PantillasCorreoBC(usuarioLog).SolicitudAntecedentes(
                                                    persona.Rows[0]["pernombre_cr"].ToString() + " " + persona.Rows[0]["perapepat_cr"].ToString(),
                                                    persona.Rows[0]["percorreo_cr"].ToString()));
                                                break;
                                            case "SOFI":
                                                Response_message = new CorreosBC(usuarioLog).EnviaCorreoApi(new PantillasCorreoBC(usuarioLog).SolicitudFirmaDoc(
                                                    persona.Rows[0]["pernombre_cr"].ToString() + " " + persona.Rows[0]["perapepat_cr"].ToString(),
                                                    persona.Rows[0]["percorreo_cr"].ToString()));
                                                break;

                                        }

                                        if (Response_message.estado != estadoRespuesta.Aprobado)
                                        {
                                            Notificacion.EstadoNotificacion = "ER";
                                            Notificacion = new NotificacionesDAC(usuarioLog).SetNotificacion(Notificacion);
                                            Notificacion.EstadoTransaccion = new ResultadoRetornoBE() { codResultado = 500, Estado = EstadoTransaccion.ERROR, Mensaje = Response_message.causaEstado, Resultado = null };
                                            return Notificacion;
                                        }
                                        else
                                        {
                                            Notificacion.EstadoNotificacion = "EN";
                                            Notificacion = new NotificacionesDAC(usuarioLog).SetNotificacion(Notificacion);
                                        }

                                        

                                        if (Notificacion.DocumentoAdjunto != null && Notificacion.DocumentoAdjunto.Documento != null && Notificacion.DocumentoAdjunto.Documento.Length > 0)
                                        {
                                            Notificacion.DocumentoAdjunto.UsuarioDigi = usuarioLog.idUsuario;
                                            
                                            // Graba documento en contenedor de documentos
                                            DocumentoBE docSave = new DocDigitalesBC(usuarioLog).GrabaDocumento(Notificacion.DocumentoAdjunto);

                                            // Graba Notificación de la Persona y Asociación de Haberes y Descuentos en caso de poseer
                                            PersonaNot = InsertaNotificacionPersona(new PersonaNotificacionBE()
                                            {
                                                CodigoNotificacion = Notificacion.CodNotificacion,
                                                CodigoPersona = destinatario.CodDestinatario,
                                                Estado = "EN",
                                                Resultado = "OK",
                                                DescripcionResultado = "Correo Enviado al Cliente",
                                                UsuarioAccion = usuarioLog.idUsuario,
                                                Antecedentes = Notificacion.Antecedentes,
                                                HDPerAsociados = Notificacion.HDPerAsociados
                                            }, 
                                            Persona.idContrato, 
                                            Notificacion.DocumentoAdjunto.IdDoc, 
                                            Notificacion.DocumentoAdjunto.TipoDoc);

                                            if (docSave.EstadoTransaccion.Estado == EstadoTransaccion.OK && docSave.IdDoc != 0)
                                            {
                                                // Si Es un Anexo o Contrato Viene del proceso de Contrato
                                                if (Notificacion.DocumentoAdjunto.TipoDoc == "ANEX" || Notificacion.DocumentoAdjunto.TipoDoc == "CONT")
                                                {
                                                    new DocDigitalesBC(usuarioLog).GrabaProceso(new ProcesoDocIndBE() { CodDocDig = docSave.IdDoc, Documento = Notificacion.DocumentoAdjunto.TipoDoc, Proceso = "CONT", Indice = "RUPE", Entidad = ((EmpresaLitleBE)HttpContext.Current.Session["EmpSel"]).idEmpresa, Valor = Persona.Rut + "-" + Persona.Dv });
                                                    new DocDigitalesBC(usuarioLog).GrabaProceso(new ProcesoDocIndBE() { CodDocDig = docSave.IdDoc, Documento = Notificacion.DocumentoAdjunto.TipoDoc, Proceso = "CONT", Indice = "COTR", Entidad = ((EmpresaLitleBE)HttpContext.Current.Session["EmpSel"]).idEmpresa, Valor = Persona.idContrato.ToString() });
                                                }
                                                //////////////////////////////////////////////////////////////////////

                                                // Indice por Defaul Proceso de Notificacion
                                                new DocDigitalesBC(usuarioLog).GrabaProceso(new ProcesoDocIndBE() { CodDocDig = docSave.IdDoc, Documento = Notificacion.DocumentoAdjunto.TipoDoc, Proceso = "NOTI", Indice = "NOPE", Entidad = ((EmpresaLitleBE)HttpContext.Current.Session["EmpSel"]).idEmpresa, Valor = PersonaNot.idPerNotif.ToString() });
                                                ActualizaNotificacionPersona(new PersonaNotificacionBE() { idPerNotif = PersonaNot.idPerNotif, FechaLectura = DateTime.MinValue, Resultado = "OK", Estado = "AD", DescripcionResultado = "Documento ingresado para su firma", UsuarioAccion = usuarioLog.idUsuario, idEntidad = ((EmpresaLitleBE)HttpContext.Current.Session["EmpSel"]).idEmpresa });

                                            }
                                            else
                                                ActualizaNotificacionPersona(new PersonaNotificacionBE() { idPerNotif = PersonaNot.idPerNotif, FechaLectura = DateTime.MinValue, Resultado = "ERR", Estado = "AD", DescripcionResultado = "Ocurrió un error al grabar el documento [" + docSave.EstadoTransaccion.Mensaje + "]", UsuarioAccion = usuarioLog.idUsuario, idEntidad = ((EmpresaLitleBE)HttpContext.Current.Session["EmpSel"]).idEmpresa });

                                        }
                                    }
                                    else
                                    {
                                        InsertaNotificacionPersona(new PersonaNotificacionBE()
                                        {
                                            CodigoNotificacion = Notificacion.CodNotificacion,
                                            CodigoPersona = destinatario.CodDestinatario,
                                            Estado = "NE",
                                            Resultado = "ERR",
                                            DescripcionResultado = "Persona no fue encontrada",
                                            UsuarioAccion = usuarioLog.idUsuario,
                                            Antecedentes = Notificacion.Antecedentes
                                        }, Persona.idContrato, Notificacion.DocumentoAdjunto.IdDoc, Notificacion.DocumentoAdjunto.TipoDoc);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                ActualizaNotificacionPersona(new PersonaNotificacionBE() { idPerNotif = PersonaNot.idPerNotif, FechaLectura = DateTime.MinValue, Resultado = "ERR", Estado = "EX", DescripcionResultado = ex.Message, UsuarioAccion = usuarioLog.idUsuario, idEntidad = ((EmpresaLitleBE)HttpContext.Current.Session["EmpSel"]).idEmpresa });
                                throw new Exception("No fue posible grabar la notificación");
                            }

                        }
                    }

                    if (Notificacion.Antecedentes != null && Notificacion.TipoNot == "SODO")
                        foreach (string Antecedente in Notificacion.Antecedentes)
                            InsertaAntecedentesSolicitud(Notificacion.CodNotificacion, Antecedente, usuarioLog.idUsuario);
                }
                else
                    throw new Exception("No fue posible grabar la notificación");

                return Notificacion;
            }
            catch (Exception ex)
            {
                Notificacion.DocumentoAdjunto.Documento    = "";
                LogLocalBE log = new LogLocalBE()
                {
                    Fecha = DateTime.Now,
                    Funcion = "NotificacionesBC",
                    Mensaje = ex.Message,
                    Metodo = "SetNotificacion",
                    Parametros = JsonConvert.SerializeObject(Notificacion),

                    Usuario = usuarioLog.idUsuario.ToString()
                };

                new LogSessionesDAC(usuarioLog).GrabaLogLocalErrores(log);
                throw new Exception(ex.Message);
            }
        }

        public ResultadoRetornoBE RescataNotificaciones(int codNotificacion, string tipoNotificacion, int CodEmpresa)
        {
            try
            {
                using (DataTable tdNotificacion = new NotificacionesDAC(usuarioLog).RescataNotificaciones(codNotificacion, tipoNotificacion, CodEmpresa).Tables[0])
                {
                    if (tdNotificacion.Rows.Count > 0)
                    {
                        using (DataTable tdFlujo = new NotificacionesDAC(usuarioLog).EstadosFlujoNotificacion(codNotificacion, 0, CodEmpresa).Tables[0])
                        {
                            tdNotificacion.Columns.Add("flowNoti", typeof(string));
                            foreach (DataRow filanot in tdNotificacion.Rows)
                                filanot["flowNoti"] = JsonConvert.SerializeObject(tdFlujo.AsEnumerable().Where(myRow => myRow.Field<int>("pnocodpno_id") == int.Parse(filanot["idNotPersona"].ToString())).CopyToDataTable());

                            return new ResultadoRetornoBE() { codResultado = 200, Estado = EstadoTransaccion.OK, Mensaje = "OK", Resultado = tdNotificacion };
                        }
                    }
                    else
                        return new ResultadoRetornoBE() { codResultado = 200, Estado = EstadoTransaccion.OK, Mensaje = "OK", Resultado = tdNotificacion };
                }
            }
            catch (Exception ex)
            {
                LogLocalBE log = new LogLocalBE()
                {
                    Fecha = DateTime.Now,
                    Funcion = "NotificacionesBC",
                    Mensaje = ex.Message,
                    Metodo = "RescataNotificaciones",
                    Parametros = "{codNotificacion:" + codNotificacion + "tipoNotificacion:" + tipoNotificacion + ",CodEmpresa:" + CodEmpresa + "}",
                    Usuario = usuarioLog.idUsuario.ToString()
                };

                new LogSessionesDAC(usuarioLog).GrabaLogLocalErrores(log);
                throw new Exception(ex.Message);
            }
        }

        private bool InsertDestinatarioNoficiacion(DestinatariosBE Destinatario)
        {
            try
            {
                if (Destinatario.CodNotificacion == 0) throw new Exception("Entidad no indicada para el ingreso de la notificación");
                if (Destinatario.CodDestinatario == 0) throw new Exception("Usuario creador no indicado");
                if (string.IsNullOrEmpty(Destinatario.TipoDestinatario)) throw new Exception("Debes indicar el nombre de la notificación");


                return new NotificacionesDAC(usuarioLog).InsertDestinatarioNoficiacion(Destinatario);

            }
            catch (Exception ex)
            {
                LogLocalBE log = new LogLocalBE()
                {
                    Fecha = DateTime.Now,
                    Funcion = "NotificacionesBC",
                    Mensaje = ex.Message,
                    Metodo = "InsertDestinatarioNoficiacion",
                    Parametros = JsonConvert.SerializeObject(Destinatario),

                    Usuario = usuarioLog.idUsuario.ToString()
                };

                new LogSessionesDAC(usuarioLog).GrabaLogLocalErrores(log);
                throw new Exception(ex.Message);
            }
        }
        public bool ActualizaDocPerNotif(int CodigoDocPerNotif, int UsuarioMod)
        {
            try
            {
                if (CodigoDocPerNotif == 0) throw new Exception("Codigo de Documento no enviado para su actualización");
                return new NotificacionesDAC(usuarioLog).ActualizaDocPerNotif(CodigoDocPerNotif, UsuarioMod);

            }
            catch (Exception ex)
            {
                LogLocalBE log = new LogLocalBE()
                {
                    Fecha = DateTime.Now,
                    Funcion = "NotificacionesBC",
                    Mensaje = ex.Message,
                    Metodo = "ActualizaAntecedentesSolicitud",
                    Parametros = JsonConvert.SerializeObject("{CodigoDocPerNotif:" + CodigoDocPerNotif + "}"),

                    Usuario = usuarioLog.idUsuario.ToString()
                };

                new LogSessionesDAC(usuarioLog).GrabaLogLocalErrores(log);
                throw new Exception(ex.Message);
            }
        }

        private bool InsertaAntecedentesSolicitud(int CodigoNotif, string Nombre, int UsuarioSol)
        {
            try
            {
                if (CodigoNotif == 0) throw new Exception("Entidad no indicada para el ingreso de la solicitud de antecedentes");
                if (string.IsNullOrEmpty(Nombre)) throw new Exception("Debes indicar el nombre del antecedente solicitado");
                if (UsuarioSol == 0) throw new Exception("Usuario solicitud no indicado");

                return new NotificacionesDAC(usuarioLog).InsertaAntecedentesSolicitud(CodigoNotif, Nombre, UsuarioSol);

            }
            catch (Exception ex)
            {
                LogLocalBE log = new LogLocalBE()
                {
                    Fecha = DateTime.Now,
                    Funcion = "NotificacionesBC",
                    Mensaje = ex.Message,
                    Metodo = "InsertaAntecedentesSolicitud",
                    Parametros = JsonConvert.SerializeObject("{CodigoNotif:" + CodigoNotif + ",Nombre:" + Nombre + ",UsuarioSol:" + UsuarioSol + "}"),

                    Usuario = usuarioLog.idUsuario.ToString()
                };

                new LogSessionesDAC(usuarioLog).GrabaLogLocalErrores(log);
                throw new Exception(ex.Message);
            }
        }
        public bool AnularNotificacion(int idNotificacion, int idEmpresa)
        {
            try
            {
                return new NotificacionesDAC(usuarioLog).AnularNotificacion(idNotificacion, idEmpresa);
            }
            catch (Exception ex)
            {
                LogLocalBE log = new LogLocalBE()
                {
                    Fecha = DateTime.Now,
                    Funcion = "NotificacionesBC",
                    Mensaje = ex.Message,
                    Metodo = "AnularNotificacion",
                    Parametros = "{idNotificacion:" + idNotificacion + ",idEmpresa:" + idEmpresa + "}",
                    Usuario = usuarioLog.idUsuario.ToString()
                };

                new LogSessionesDAC(usuarioLog).GrabaLogLocalErrores(log);
                throw new Exception(ex.Message);
            }
        }

        public ResultadoRetornoBE ReenviaNotificacion(int idNotificacion, int idEmpresa)
        {
            try
            {
                ResponseSendMail Response_message = new ResponseSendMail() { estado = estadoRespuesta.Error };
                ResultadoRetornoBE ResNot = RescataNotificaciones(idNotificacion, null, idEmpresa);
                string CorreoEnvio = "";
                if (ResNot.Estado == EstadoTransaccion.OK && ((DataTable)ResNot.Resultado) != null && ((DataTable)ResNot.Resultado).Rows.Count > 0)

                    using (DataTable dtNotificacion = (DataTable)ResNot.Resultado)
                    {
                        using (DataTable persona = new PersonaBC(usuarioLog).RescataPersona(idEmpresa, null, int.Parse(dtNotificacion.Rows[0]["personaCod"].ToString())))
                        {
                            switch (dtNotificacion.Rows[0]["idtipo"].ToString().ToUpper())
                            {
                                case "SODO":
                                    Response_message = new CorreosBC(usuarioLog).EnviaCorreoApi(new PantillasCorreoBC(usuarioLog).SolicitudAntecedentes(
                                        persona.Rows[0]["pernombre_cr"].ToString() + " " + persona.Rows[0]["perapepat_cr"].ToString(),
                                        persona.Rows[0]["percorreo_cr"].ToString()));
                                    break;
                                case "SOFI":
                                    Response_message = new CorreosBC(usuarioLog).EnviaCorreoApi(new PantillasCorreoBC(usuarioLog).SolicitudFirmaDoc(
                                        persona.Rows[0]["pernombre_cr"].ToString() + " " + persona.Rows[0]["perapepat_cr"].ToString(),
                                        persona.Rows[0]["percorreo_cr"].ToString()));
                                    break;

                            }

                            CorreoEnvio = persona.Rows[0]["percorreo_cr"].ToString();
                        }
                    }

                if (Response_message.estado != estadoRespuesta.Aprobado)
                    return new ResultadoRetornoBE() { codResultado = 500, Estado = EstadoTransaccion.ERROR, Mensaje = Response_message.causaEstado, Resultado = null };
                else
                    return new ResultadoRetornoBE() { codResultado = 200, Estado = EstadoTransaccion.OK, Mensaje = "OK", Resultado = CorreoEnvio };

            }
            catch (Exception ex)
            {
                LogLocalBE log = new LogLocalBE()
                {
                    Fecha = DateTime.Now,
                    Funcion = "NotificacionesBC",
                    Mensaje = ex.Message,
                    Metodo = "ReenviaNotificacion",
                    Parametros = "{idNotificacion:" + idNotificacion + ",idEmpresa:" + idEmpresa + "}",
                    Usuario = usuarioLog.idUsuario.ToString()
                };

                new LogSessionesDAC(usuarioLog).GrabaLogLocalErrores(log);
                return new ResultadoRetornoBE() { codResultado = 500, Estado = EstadoTransaccion.ERROR, Mensaje = ex.Message, Resultado = null };
            }
        }

        public NotificacionBE SetNotificacionAntecedentes(NotificacionBE Notificacion, int Empresa, bool Transaccional, PersonaLteBE persona)
        {
            try
            {
                if (Notificacion.CodEntidad == 0) throw new Exception("Entidad no indicada para el ingreso de la notificación");
                if (Notificacion.UsuarioCreador == 0) throw new Exception("Usuario creador no indicado");

                Notificacion.NombreNotificacion = "Solicitud de Antecedentes Contrato";
                Notificacion.FechaNot = DateTime.Now;
                Notificacion.TipoNot = "SODO";
                Notificacion.MensajeRespuesta = "N";
                Notificacion.MensajeEntrada = "Se Solicitan los siguientes Documentos para el procesos de contratación";
                Notificacion.TomaConocimiento = "N";

                return new NotificacionesBC(usuarioLog).SetNotificacion(Notificacion, Transaccional, persona);
            }
            catch (Exception ex)
            {
                LogLocalBE log = new LogLocalBE()
                {
                    Fecha = DateTime.Now,
                    Funcion = "NotificacionesBC",
                    Mensaje = ex.Message,
                    Metodo = "SetNotificacion",
                    Parametros = JsonConvert.SerializeObject(Notificacion),

                    Usuario = usuarioLog.idUsuario.ToString()
                };

                new LogSessionesDAC(usuarioLog).GrabaLogLocalErrores(log);
                throw new Exception(ex.Message);
            }
        }

        public DataTable RescataAntecedentesNotificacion(int idPersona, int idEmpresa)
        {
            try
            {
                return new NotificacionesDAC(usuarioLog).RescataAntecedentesNotificacion(idPersona, idEmpresa);
            }
            catch (Exception ex)
            {
                LogLocalBE log = new LogLocalBE()
                {
                    Fecha = DateTime.Now,
                    Funcion = "NotificacionesBC",
                    Mensaje = ex.Message,
                    Metodo = "RescataAntecedentesNotificacion",
                    Parametros = "{idPersona:" + idPersona + ",idEmpresa:" + idEmpresa + "}",
                    Usuario = usuarioLog.idUsuario.ToString()
                };

                new LogSessionesDAC(usuarioLog).GrabaLogLocalErrores(log);
                throw new Exception(ex.Message);
            }
        }

        public DataSet NotificacionesxPersona(int idNotificacion, int idPersona, int idEmpresa, bool lectura)
        {
            try
            {
                using (DataSet dsNotificaciones = new NotificacionesDAC(usuarioLog).NotificacionesxPersona(idNotificacion, idPersona, idEmpresa))
                {
                    if (idNotificacion != 0 &&
                        dsNotificaciones.Tables.Count > 0 &&
                        dsNotificaciones.Tables[0].Rows.Count > 0 &&
                        dsNotificaciones.Tables[0].Rows[0]["pnofeclec_fc"].ToString().Length == 0 && lectura)
                    {

                        ActualizaNotificacionPersona(
                            new PersonaNotificacionBE()
                            {
                                idPerNotif = Int32.Parse(dsNotificaciones.Tables[0].Rows[0]["idNotPer"].ToString()),
                                FechaLectura = DateTime.Now,
                                Resultado = "OK",
                                Estado = "LE",
                                DescripcionResultado = "Notificación Leida por el trabajador",
                                UsuarioAccion = usuarioLog.idUsuario,
                                idEntidad = idEmpresa
                            });
                    }

                    using (DataTable tdFlujo = new NotificacionesDAC(usuarioLog).EstadosFlujoNotificacion(Int32.Parse(dsNotificaciones.Tables[0].Rows[0]["idNotPer"].ToString()), idPersona, idEmpresa).Tables[0])
                    {
                        dsNotificaciones.Tables[0].Columns.Add("flowNoti", typeof(string));
                        foreach (DataRow filanot in dsNotificaciones.Tables[0].Rows)
                        {
                            try
                            {
                                filanot["flowNoti"] = JsonConvert.SerializeObject(tdFlujo.AsEnumerable().Where(myRow => myRow.Field<int>("pnocodpno_id") == int.Parse(filanot["idNotPer"].ToString())).CopyToDataTable());
                            }
                            catch { }
                        }
                    }
                    return dsNotificaciones;
                }
            }
            catch (Exception ex)
            {
                LogLocalBE log = new LogLocalBE()
                {
                    Fecha = DateTime.Now,
                    Funcion = "NotificacionesBC",
                    Mensaje = ex.Message,
                    Metodo = "NotificacionesxPersona",
                    Parametros = "{idNotificacion:" + idNotificacion + ",idPersona:" + idPersona + ",idEmpresa:" + idEmpresa + "}",
                    Usuario = usuarioLog.idUsuario.ToString()
                };

                new LogSessionesDAC(usuarioLog).GrabaLogLocalErrores(log);
                throw new Exception(ex.Message);
            }
        }

        private PersonaNotificacionBE InsertaNotificacionPersona(PersonaNotificacionBE NotificacionPersona, int codCont, int codDoc, string TipFirma)
        {
            try
            {
                if (NotificacionPersona.CodigoNotificacion == 0) throw new Exception("Debes indicar la notificación asociada");
                if (NotificacionPersona.CodigoPersona == 0) throw new Exception("Debes inficar la persona asociada a la notificación");
                if (string.IsNullOrEmpty(NotificacionPersona.Estado)) throw new Exception("Debes ingresar el estado de la notificación");
                if (string.IsNullOrEmpty(NotificacionPersona.Resultado)) throw new Exception("Debes indicar el resultado de la notificación");

                PersonaNotificacionBE resNotificacionPer = new NotificacionesDAC(usuarioLog).InsertaNotificacionPersona(NotificacionPersona);

                if (NotificacionPersona.HDPerAsociados != null && NotificacionPersona.HDPerAsociados.Count > 0)
                    InsertaHdPerNotificacion(NotificacionPersona.idPerNotif, NotificacionPersona.HDPerAsociados, codCont, codDoc, TipFirma);

                return resNotificacionPer;
            }
            catch (Exception ex)
            {
                LogLocalBE log = new LogLocalBE()
                {
                    Fecha = DateTime.Now,
                    Funcion = "NotificacionesBC",
                    Mensaje = ex.Message,
                    Metodo = "InsertaNotificacionPersona",
                    Parametros = "{NotificacionPersona:" + JsonConvert.SerializeObject(NotificacionPersona) +
                    ",codCont:" + codCont +
                    ",CodDoc:" + codDoc +
                    ",TipFirma:" + TipFirma + "}",

                    Usuario = usuarioLog.idUsuario.ToString()
                };

                new LogSessionesDAC(usuarioLog).GrabaLogLocalErrores(log);
                throw new Exception(ex.Message);
            }
        }

        public bool InsertaHdPerNotificacion(int codNotificacion, List<int> HDPerAsociados, int codCont, int CodDoc, string TipFirma)
        {
            try
            {
                foreach (int HdPerAsociado in HDPerAsociados)
                    new NotificacionesDAC(usuarioLog).InsertaHdPerNotificacion(codNotificacion, HdPerAsociado, codCont, CodDoc, TipFirma);

                return true;
            }
            catch (Exception ex)
            {
                LogLocalBE log = new LogLocalBE()
                {
                    Fecha = DateTime.Now,
                    Funcion = "NotificacionesBC",
                    Mensaje = ex.Message,
                    Metodo = "InsertaHdPerNotificacion",
                    Parametros = JsonConvert.SerializeObject("{codNotificacion:" + codNotificacion +
                    ",HDPerAsociados:" + JsonConvert.SerializeObject(HDPerAsociados) +
                    ",codCont:" + codCont +
                    ",CodDoc:" + CodDoc +
                    ",TipFirma:" + TipFirma +
                    ",usuario:" + usuarioLog.idUsuario + "}"),
                    Usuario = usuarioLog.idUsuario.ToString()
                };

                new LogSessionesDAC(usuarioLog).GrabaLogLocalErrores(log);
                throw new Exception(ex.Message);
            }
        }
        public bool FirmaHdPerNotificacion(int codNotificacion, int codPersona, int CodContrato, string TokenFirma)
        {
            try
            {
                new NotificacionesDAC(usuarioLog).FirmaHdPerNotificacion(codNotificacion, codPersona, CodContrato, TokenFirma);
                return true;
            }
            catch (Exception ex)
            {
                LogLocalBE log = new LogLocalBE()
                {
                    Fecha = DateTime.Now,
                    Funcion = "NotificacionesBC",
                    Mensaje = ex.Message,
                    Metodo = "FirmaHdPerNotificacion",
                    Parametros = JsonConvert.SerializeObject("{codNotificacion:" + codNotificacion +
                    ",codPersona:" + codPersona +
                    ",CodContrato:" + CodContrato +
                    ",usuario:" + usuarioLog.idUsuario +
                    ",TokenFirma:" + TokenFirma + "}"),
                    Usuario = usuarioLog.idUsuario.ToString()
                };

                new LogSessionesDAC(usuarioLog).GrabaLogLocalErrores(log);
                throw new Exception("No fue posible firmar el documento");
            }
        }

        public bool ActualizaNotificacionPersona(PersonaNotificacionBE personaNotificacion)
        {
            try
            {
                return new NotificacionesDAC(usuarioLog).ActualizaNotificacionPersona(personaNotificacion);
            }
            catch (Exception ex)
            {
                LogLocalBE log = new LogLocalBE()
                {
                    Fecha = DateTime.Now,
                    Funcion = "NotificacionesBC",
                    Mensaje = ex.Message,
                    Metodo = "ActualizaNotificacionPersona",
                    Parametros = JsonConvert.SerializeObject(personaNotificacion),
                    Usuario = usuarioLog.idUsuario.ToString()
                };

                new LogSessionesDAC(usuarioLog).GrabaLogLocalErrores(log);
                throw new Exception(ex.Message);
            }
        }

        public DataSet ResumenNotificaciones(int idEmpresa)
        {
            try
            {
                using (DataSet dsNotificaciones = new NotificacionesDAC(usuarioLog).ResumenNotificaciones(idEmpresa))
                {
                    return dsNotificaciones;
                }
            }
            catch (Exception ex)
            {
                LogLocalBE log = new LogLocalBE()
                {
                    Fecha = DateTime.Now,
                    Funcion = "NotificacionesBC",
                    Mensaje = ex.Message,
                    Metodo = "ResumenNotificaciones",
                    Parametros = "{idEmpresa:" + idEmpresa + "}",
                    Usuario = usuarioLog.idUsuario.ToString()
                };

                new LogSessionesDAC(usuarioLog).GrabaLogLocalErrores(log);
                throw new Exception(ex.Message);
            }
        }

        public DataSet EstadosFlujoNotificacion(int idNotificacion, int idPersona, int idEmpresa)
        {
            try
            {
                return new NotificacionesDAC(usuarioLog).EstadosFlujoNotificacion(idNotificacion, idPersona, idEmpresa);
            }
            catch (Exception ex)
            {
                LogLocalBE log = new LogLocalBE()
                {
                    Fecha = DateTime.Now,
                    Funcion = "NotificacionesBC",
                    Mensaje = ex.Message,
                    Metodo = "EstadosFlujoNotificacion",
                    Parametros = "{idNotificacion:" + idNotificacion + ",idPersona:" + idPersona + "}",
                    Usuario = usuarioLog.idUsuario.ToString()
                };

                new LogSessionesDAC(usuarioLog).GrabaLogLocalErrores(log);
                throw new Exception(ex.Message);
            }
        }

        public bool CierraNotificacion(int Notificacion, UsuarioBE Persona)
        {
            try
            {
                DataSet dsRes = NotificacionesxPersona(Notificacion, Persona.idUsuario, Persona.CodEntidad, false);
                if (dsRes.Tables[2].Select("fecenv is null").Length > 0)
                    throw new Exception("Para confirmar la información debe enviar la totalidad de documentos solicitados");

                return ActualizaNotificacionPersona(new PersonaNotificacionBE() { idPerNotif = Notificacion, Estado = "RS", Resultado = "OK", DescripcionResultado = "Usuario Confirma Envio de Docuementos", UsuarioAccion = usuarioLog.idUsuario, idEntidad = Persona.CodEntidad, FechaRespuesta = DateTime.Now });
            }
            catch (Exception ex)
            {
                LogLocalBE log = new LogLocalBE()
                {
                    Fecha = DateTime.Now,
                    Funcion = "NotificacionesBC",
                    Mensaje = ex.Message,
                    Metodo = "CierraNotificacion",
                    Parametros = "{Notificacion:" + Notificacion + "}",
                    Usuario = usuarioLog.idUsuario.ToString()
                };

                new LogSessionesDAC(usuarioLog).GrabaLogLocalErrores(log);
                throw new Exception(ex.Message);
            }
        }

        public DataTable ValidaNotificacionPendiente(int CodContrato, int codPersona, string TipoDocPen)
        {
            try
            {
                return new NotificacionesDAC(usuarioLog).ValidaNotificacionPendiente(CodContrato, codPersona, TipoDocPen);
            }
            catch (Exception ex)
            {
                LogLocalBE log = new LogLocalBE()
                {
                    Fecha = DateTime.Now,
                    Funcion = "NotificacionesBC",
                    Mensaje = ex.Message,
                    Metodo = "ValidaNotificacionPendiente",
                    Parametros = JsonConvert.SerializeObject("{CodContrato:" + CodContrato +
                    ",codPersona:" + codPersona +
                    ",TipoDocPen:" + TipoDocPen + "}"),
                    Usuario = usuarioLog.idUsuario.ToString()
                };

                new LogSessionesDAC(usuarioLog).GrabaLogLocalErrores(log);
                throw new Exception(ex.Message);
            }
        }

        public void Anula_NotificacionPendiente(int CodContrato, int codPersona, string TipoDocPen)
        {
            try
            {
                new NotificacionesDAC(usuarioLog).Anula_NotificacionPendiente(CodContrato, codPersona, TipoDocPen);
            }
            catch (Exception ex)
            {
                LogLocalBE log = new LogLocalBE()
                {
                    Fecha = DateTime.Now,
                    Funcion = "NotificacionesBC",
                    Mensaje = ex.Message,
                    Metodo = "Anula_NotificacionPendiente",
                    Parametros = JsonConvert.SerializeObject("{CodContrato:" + CodContrato +
                    ",codPersona:" + codPersona +
                    ",TipoDocPen:" + TipoDocPen + 
                    ",usuaccion:" + usuarioLog.idUsuario + "}"),
                    Usuario = usuarioLog.idUsuario.ToString()
                };

                new LogSessionesDAC(usuarioLog).GrabaLogLocalErrores(log);
                throw new Exception(ex.Message);
            }
        }
    }
}