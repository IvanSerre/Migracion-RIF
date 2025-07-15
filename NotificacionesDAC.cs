using Microsoft.SqlServer.Server;
using Newtonsoft.Json;
using RIF_Logic_Access.BE;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace RIF_Logic_Access.DAC
{
    public class NotificacionesDAC
    {
        protected UsuarioBE usuarioLog = new UsuarioBE();
        string strConexion = ConfigurationManager.ConnectionStrings["SQLCONRIF"].ConnectionString;
        public NotificacionesDAC(UsuarioBE objUsu)
        {
            usuarioLog = objUsu;
        }
        public DataTable RescataCatalogoNotificaciones(string codCatNotificacion)
        {
            try
            {
                using (SqlConnection conexion = new SqlConnection(strConexion))
                {
                    conexion.Open();
                    using (SqlCommand command = new SqlCommand { Connection = conexion, CommandType = CommandType.StoredProcedure, CommandText = "Get_CatalogoNotificaciones", CommandTimeout = 10 })
                    {

                        if (!string.IsNullOrEmpty(codCatNotificacion))
                            command.Parameters.AddWithValue("@codCatNot", codCatNotificacion);
                        using (SqlDataAdapter Adapter = new SqlDataAdapter(command))
                        {
                            DataTable dt = new DataTable();
                            Adapter.Fill(dt);
                            return dt;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogLocalBE log = new LogLocalBE()
                {
                    Fecha = DateTime.Now,
                    Funcion = "NotificacionesDAC",
                    Mensaje = ex.Message,
                    Metodo = "RescataCatalogoNotificaciones",
                    Parametros = "{codCatNotificacion:" + codCatNotificacion + "}",
                    Usuario = usuarioLog.idUsuario.ToString()
                };

                new LogSessionesDAC(usuarioLog).GrabaLogLocalErrores(log);
                throw new Exception(ex.Message);
            }
        }
        public NotificacionBE SetNotificacion(NotificacionBE Notificacion)
        {
            try
            {
                using (SqlConnection conexion = new SqlConnection(strConexion))
                {
                    conexion.Open();
                    using (SqlCommand command = new SqlCommand { Connection = conexion, CommandType = CommandType.StoredProcedure, CommandText = "Set_Notificacion", CommandTimeout = 10 })
                    {

                        command.Parameters.AddWithValue("@Codigo", Notificacion.CodNotificacion);
                        command.Parameters.AddWithValue("@Entidad", Notificacion.CodEntidad);
                        command.Parameters.AddWithValue("@Nombre", Notificacion.NombreNotificacion.Trim());
                        command.Parameters.AddWithValue("@FechaNotificacion", Notificacion.FechaNot);
                        command.Parameters.AddWithValue("@TipoNotificacion", Notificacion.TipoNot);
                        command.Parameters.AddWithValue("@MensajeRespuesta", Notificacion.MensajeRespuesta);
                        command.Parameters.AddWithValue("@TomaConocimiento", Notificacion.TomaConocimiento);
                        command.Parameters.AddWithValue("@UsuCreador", Notificacion.UsuarioCreador);


                        if (!string.IsNullOrEmpty(Notificacion.MensajeEntrada))
                            command.Parameters.AddWithValue("@MensajeEntrada", Notificacion.MensajeEntrada);

                        if (Notificacion.DocumentoAdjunto != null && !string.IsNullOrEmpty(Notificacion.DocumentoAdjunto.TipoDoc))
                            command.Parameters.AddWithValue("@TipoDoc", Notificacion.DocumentoAdjunto.TipoDoc);

                        if (!string.IsNullOrEmpty(Notificacion.EstadoNotificacion))
                            command.Parameters.AddWithValue("@EstNot", Notificacion.EstadoNotificacion);

                        var paramId = new SqlParameter("@Id", SqlDbType.Int);
                        command.Parameters.Add(paramId).Direction = ParameterDirection.Output;

                        command.ExecuteNonQuery();
                        Notificacion.CodNotificacion = Int32.Parse(command.Parameters["@Id"].Value.ToString().Trim());
                        Notificacion.EstadoTransaccion = new ResultadoRetornoBE() { codResultado = 200, Estado = EstadoTransaccion.OK, Mensaje = "" };
                        return Notificacion;
                    }
                }
            }
            catch (Exception ex)
            {
                LogLocalBE log = new LogLocalBE()
                {
                    Fecha = DateTime.Now,
                    Funcion = "NotificacionesDAC",
                    Mensaje = ex.Message,
                    Metodo = "SetNotificacion",
                    Parametros = JsonConvert.SerializeObject(Notificacion),
                    Usuario = usuarioLog.idUsuario.ToString()
                };

                new LogSessionesDAC(usuarioLog).GrabaLogLocalErrores(log);
                throw new Exception(ex.Message);
            }
        }
        public DataSet RescataNotificaciones(int codNotificacion, string tipoNotificacion, int CodEmpresa)
        {
            try
            {
                using (SqlConnection conexion = new SqlConnection(strConexion))
                {
                    conexion.Open();
                    using (SqlCommand command = new SqlCommand { Connection = conexion, CommandType = CommandType.StoredProcedure, CommandText = "Get_Notificacion", CommandTimeout = 10 })
                    {

                        if (codNotificacion != 0)
                            command.Parameters.AddWithValue("@idNotificacion", codNotificacion);
                        if (!string.IsNullOrEmpty(tipoNotificacion))
                            command.Parameters.AddWithValue("@tipoNotificacion", tipoNotificacion);

                        command.Parameters.AddWithValue("@idEmpresa", CodEmpresa);

                        using (SqlDataAdapter Adapter = new SqlDataAdapter(command))
                        {
                            DataSet dt = new DataSet();
                            Adapter.Fill(dt);
                            return dt;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogLocalBE log = new LogLocalBE()
                {
                    Fecha = DateTime.Now,
                    Funcion = "NotificacionesDAC",
                    Mensaje = ex.Message,
                    Metodo = "RescataNotificaciones",
                    Parametros = "{codNotificacion:" + codNotificacion + ",CodEmpresa:" + CodEmpresa + "}",
                    Usuario = usuarioLog.idUsuario.ToString()
                };

                new LogSessionesDAC(usuarioLog).GrabaLogLocalErrores(log);
                throw new Exception(ex.Message);
            }
        }
        public bool InsertDestinatarioNoficiacion(DestinatariosBE Destinatarios)
        {
            try
            {
                using (SqlConnection conexion = new SqlConnection(strConexion))
                {
                    conexion.Open();
                    using (SqlCommand command = new SqlCommand { Connection = conexion, CommandType = CommandType.StoredProcedure, CommandText = "Insert_DestinatariosNotificacion", CommandTimeout = 10 })
                    {

                        command.Parameters.AddWithValue("@CodigoNotif", Destinatarios.CodNotificacion);
                        command.Parameters.AddWithValue("@CodDestinatario", Destinatarios.CodDestinatario);
                        command.Parameters.AddWithValue("@TipoDestinatario", Destinatarios.TipoDestinatario.Trim());
                        command.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                LogLocalBE log = new LogLocalBE()
                {
                    Fecha = DateTime.Now,
                    Funcion = "NotificacionesDAC",
                    Mensaje = ex.Message,
                    Metodo = "InsertDestinatarioNoficiacion",
                    Parametros = JsonConvert.SerializeObject(Destinatarios),
                    Usuario = usuarioLog.idUsuario.ToString()
                };

                new LogSessionesDAC(usuarioLog).GrabaLogLocalErrores(log);
                throw new Exception(ex.Message);
            }
        }

        public bool InsertaAntecedentesSolicitud(int CodigoNotif, string Nombre, int UsuarioSol)
        {
            try
            {
                using (SqlConnection conexion = new SqlConnection(strConexion))
                {
                    conexion.Open();
                    using (SqlCommand command = new SqlCommand { Connection = conexion, CommandType = CommandType.StoredProcedure, CommandText = "Insert_AntecedentesSolNotificacion", CommandTimeout = 10 })
                    {

                        command.Parameters.AddWithValue("@CodigoNotif", CodigoNotif);
                        command.Parameters.AddWithValue("@DocumentoSol", Nombre.Trim());
                        command.Parameters.AddWithValue("@UsuSol", UsuarioSol);
                        command.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                LogLocalBE log = new LogLocalBE()
                {
                    Fecha = DateTime.Now,
                    Funcion = "NotificacionesDAC",
                    Mensaje = ex.Message,
                    Metodo = "InsertaAntecedentesSolicitud",
                    Parametros = JsonConvert.SerializeObject("{CodigoNotif:" + CodigoNotif + ",Nombre:" + Nombre + ",UsuarioSol:" + UsuarioSol + "}"),
                    Usuario = usuarioLog.idUsuario.ToString()
                };

                new LogSessionesDAC(usuarioLog).GrabaLogLocalErrores(log);
                throw new Exception(ex.Message);
            }
        }

        public bool ActualizaDocPerNotif(int CodigoDocPerNotif, int UsuarioMod)
        {
            // ActualizaAntecedentesSolicitud
            try
            {
                using (SqlConnection conexion = new SqlConnection(strConexion))
                {
                    conexion.Open();
                    using (SqlCommand command = new SqlCommand { Connection = conexion, CommandType = CommandType.StoredProcedure, CommandText = "Upd_DocumentosPersonaNotif", CommandTimeout = 10 })
                    {
                        command.Parameters.AddWithValue("@CodigoDocPerNotif", CodigoDocPerNotif);
                        command.Parameters.AddWithValue("@UsuarioAccion", UsuarioMod);
                        command.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                LogLocalBE log = new LogLocalBE()
                {
                    Fecha = DateTime.Now,
                    Funcion = "NotificacionesDAC",
                    Mensaje = ex.Message,
                    Metodo = "ActualizaDocPerNotif",
                    Parametros = JsonConvert.SerializeObject("{CodigoDocPerNotif:" + CodigoDocPerNotif + "}"),
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
                using (SqlConnection conexion = new SqlConnection(strConexion))
                {
                    conexion.Open();
                    using (SqlCommand command = new SqlCommand { Connection = conexion, CommandType = CommandType.StoredProcedure, CommandText = "Anula_Notificacion", CommandTimeout = 10 })
                    {
                        command.Parameters.AddWithValue("@CodigoNotif", idNotificacion);
                        command.Parameters.AddWithValue("@CodEntidad", idEmpresa);
                        command.Parameters.AddWithValue("@CorUsuario", usuarioLog.idUsuario);
                        command.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                LogLocalBE log = new LogLocalBE()
                {
                    Fecha = DateTime.Now,
                    Funcion = "NotificacionesDAC",
                    Mensaje = ex.Message,
                    Metodo = "AnularNotificacion",
                    Parametros = "{idNotificacion:" + idNotificacion + ",idEmpresa:" + idEmpresa + "}",
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
                using (SqlConnection conexion = new SqlConnection(strConexion))
                {
                    conexion.Open();
                    using (SqlCommand command = new SqlCommand { Connection = conexion, CommandType = CommandType.StoredProcedure, CommandText = "Get_Antecedentes_Notificacion_Persona", CommandTimeout = 10 })
                    {

                        command.Parameters.AddWithValue("@idPersona", idPersona);
                        command.Parameters.AddWithValue("@idEmpresa", idEmpresa);

                        using (SqlDataAdapter Adapter = new SqlDataAdapter(command))
                        {
                            DataTable dt = new DataTable();
                            Adapter.Fill(dt);
                            return dt;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogLocalBE log = new LogLocalBE()
                {
                    Fecha = DateTime.Now,
                    Funcion = "NotificacionesDAC",
                    Mensaje = ex.Message,
                    Metodo = "RescataAntecedentesNotificacion",
                    Parametros = "{idPersona:" + idPersona + ",idEmpresa:" + idEmpresa + "}",
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
                using (SqlConnection conexion = new SqlConnection(strConexion))
                {
                    conexion.Open();
                    using (SqlCommand command = new SqlCommand { Connection = conexion, CommandType = CommandType.StoredProcedure, CommandText = "Get_Notificacion_Estados", CommandTimeout = 10 })
                    {
                        if (idNotificacion != 0)
                            command.Parameters.AddWithValue("@idNotificacion", idNotificacion);
                        if (idPersona != 0)
                            command.Parameters.AddWithValue("@idPersona", idPersona);

                        command.Parameters.AddWithValue("@idEmpresa", idEmpresa);

                        using (SqlDataAdapter Adapter = new SqlDataAdapter(command))
                        {
                            DataSet ds = new DataSet();
                            Adapter.Fill(ds);

                            //LogLocalBE log = new LogLocalBE()
                            //{
                            //    Fecha = DateTime.Now,
                            //    Funcion = "NotificacionesDAC",
                            //    Mensaje = "Revisa Retorno DataSet",
                            //    Metodo = "EstadosFlujoNotificacion",
                            //    Parametros = JsonConvert.SerializeObject(ds),
                            //    Usuario = usuarioLog.idUsuario.ToString()
                            //};

                            //new LogSessionesDAC(usuarioLog).GrabaLogLocalErrores(log);

                            return ds;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogLocalBE log = new LogLocalBE()
                {
                    Fecha = DateTime.Now,
                    Funcion = "NotificacionesDAC",
                    Mensaje = "Revisa Retorno DataSet",
                    Metodo = "EstadosFlujoNotificacion",
                    Parametros = "{idNotificacion:" + idNotificacion + ",idPersona:" + idPersona + ",idEmpresa:" + idEmpresa + "}",
                    Usuario = usuarioLog.idUsuario.ToString()
                };

                new LogSessionesDAC(usuarioLog).GrabaLogLocalErrores(log);
                throw new Exception(ex.Message);
            }
        }
        public DataSet NotificacionesxPersona(int idNotificacion, int idPersona, int idEmpresa)
        {
            try
            {
                using (SqlConnection conexion = new SqlConnection(strConexion))
                {
                    conexion.Open();
                    using (SqlCommand command = new SqlCommand { Connection = conexion, CommandType = CommandType.StoredProcedure, CommandText = "Get_NotificacionxPersona", CommandTimeout = 10 })
                    {
                        if (idNotificacion != 0)
                            command.Parameters.AddWithValue("@idNotificacion", idNotificacion);

                        command.Parameters.AddWithValue("@idPersona", idPersona);
                        command.Parameters.AddWithValue("@idEmpresa", idEmpresa);

                        using (SqlDataAdapter Adapter = new SqlDataAdapter(command))
                        {
                            DataSet ds = new DataSet();
                            Adapter.Fill(ds);

                            //LogLocalBE log = new LogLocalBE()
                            //{
                            //    Fecha = DateTime.Now,
                            //    Funcion = "NotificacionesDAC",
                            //    Mensaje = "Revisa Retorno DataSet",
                            //    Metodo = "NotificacionesxPersona",
                            //    Parametros = JsonConvert.SerializeObject(ds),
                            //    Usuario = usuarioLog.idUsuario.ToString()
                            //};

                            //new LogSessionesDAC(usuarioLog).GrabaLogLocalErrores(log);

                            return ds;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogLocalBE log = new LogLocalBE()
                {
                    Fecha = DateTime.Now,
                    Funcion = "NotificacionesDAC",
                    Mensaje = ex.Message,
                    Metodo = "NotificacionesxPersona",
                    Parametros = "{idNotificacion:" + idNotificacion + ",idPersona:" + idPersona + ",idEmpresa:" + idEmpresa + "}",
                    Usuario = usuarioLog.idUsuario.ToString()
                };

                new LogSessionesDAC(usuarioLog).GrabaLogLocalErrores(log);
                throw new Exception(ex.Message);
            }
        }
        public PersonaNotificacionBE InsertaNotificacionPersona(PersonaNotificacionBE NotificacionPersona)
        {
            try
            {
                using (SqlConnection conexion = new SqlConnection(strConexion))
                {
                    conexion.Open();
                    using (SqlCommand command = new SqlCommand { Connection = conexion, CommandType = CommandType.StoredProcedure, CommandText = "Insert_PersonaNotificacion", CommandTimeout = 10 })
                    {
                        command.Parameters.AddWithValue("@CodigoNotif", NotificacionPersona.CodigoNotificacion);
                        command.Parameters.AddWithValue("@CodigoPersona", NotificacionPersona.CodigoPersona);
                        command.Parameters.AddWithValue("@Estado", NotificacionPersona.Estado);
                        command.Parameters.AddWithValue("@ResultNot", NotificacionPersona.Resultado);
                        command.Parameters.AddWithValue("@UsuAccion", NotificacionPersona.UsuarioAccion);
                        command.Parameters.AddWithValue("@DesPerNot", NotificacionPersona.DescripcionResultado);
                        if (NotificacionPersona.Antecedentes != null && NotificacionPersona.Antecedentes.Count > 0)
                            command.Parameters.AddWithValue("@AntecedentesNot", string.Join(",", NotificacionPersona.Antecedentes.ToArray()));

                        var paramId = new SqlParameter("@IdCodPerNotif", SqlDbType.Int);
                        command.Parameters.Add(paramId).Direction = ParameterDirection.Output;
                        command.ExecuteNonQuery();
                        NotificacionPersona.idPerNotif = Int32.Parse(command.Parameters["@IdCodPerNotif"].Value.ToString().Trim());

                        return NotificacionPersona;
                    }
                }
            }
            catch (Exception ex)
            {
                LogLocalBE log = new LogLocalBE()
                {
                    Fecha = DateTime.Now,
                    Funcion = "NotificacionesDAC",
                    Mensaje = ex.Message,
                    Metodo = "InsertaNotificacionPersona",
                    Parametros = JsonConvert.SerializeObject(NotificacionPersona),
                    Usuario = usuarioLog.idUsuario.ToString()
                };

                new LogSessionesDAC(usuarioLog).GrabaLogLocalErrores(log);
                throw new Exception(ex.Message);
            }
        }
        public bool ActualizaNotificacionPersona(PersonaNotificacionBE personaNotificacion)
        {
            try
            {
                using (SqlConnection conexion = new SqlConnection(strConexion))
                {
                    conexion.Open();
                    using (SqlCommand command = new SqlCommand { Connection = conexion, CommandType = CommandType.StoredProcedure, CommandText = "Update_NotificacionPersona", CommandTimeout = 10 })
                    {
                        command.Parameters.AddWithValue("@Codigo", personaNotificacion.idPerNotif);
                        command.Parameters.AddWithValue("@Entidad", personaNotificacion.idEntidad);

                        if (personaNotificacion.FechaLectura != DateTime.MinValue)
                            command.Parameters.AddWithValue("@FechaLectura", personaNotificacion.FechaLectura);
                        if (personaNotificacion.FechaRespuesta != DateTime.MinValue)
                            command.Parameters.AddWithValue("@FechaRespuesta", personaNotificacion.FechaRespuesta);
                        if (!string.IsNullOrEmpty(personaNotificacion.TextoRespuesta))
                            command.Parameters.AddWithValue("@TextoRespuesta", personaNotificacion.TextoRespuesta);
                        if (!string.IsNullOrEmpty(personaNotificacion.ArchivoRespuesta))
                            command.Parameters.AddWithValue("@ArchivoRespuesta", personaNotificacion.ArchivoRespuesta);
                        if (!string.IsNullOrEmpty(personaNotificacion.Estado))
                            command.Parameters.AddWithValue("@EstadoNotificacion", personaNotificacion.Estado);

                        command.Parameters.AddWithValue("@UsuarioAccion", personaNotificacion.UsuarioAccion);
                        if (!string.IsNullOrEmpty(personaNotificacion.Resultado))
                            command.Parameters.AddWithValue("@ResultNot", personaNotificacion.Resultado);
                        if (!string.IsNullOrEmpty(personaNotificacion.DescripcionResultado))
                            command.Parameters.AddWithValue("@DesPerNot", personaNotificacion.DescripcionResultado);
                        command.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                LogLocalBE log = new LogLocalBE()
                {
                    Fecha = DateTime.Now,
                    Funcion = "NotificacionesDAC",
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
                using (SqlConnection conexion = new SqlConnection(strConexion))
                {
                    conexion.Open();
                    using (SqlCommand command = new SqlCommand { Connection = conexion, CommandType = CommandType.StoredProcedure, CommandText = "Resumen_Notificaciones", CommandTimeout = 10 })
                    {
                        command.Parameters.AddWithValue("@idEmpresa", idEmpresa);

                        using (SqlDataAdapter Adapter = new SqlDataAdapter(command))
                        {
                            DataSet ds = new DataSet();
                            Adapter.Fill(ds);
                            return ds;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogLocalBE log = new LogLocalBE()
                {
                    Fecha = DateTime.Now,
                    Funcion = "NotificacionesDAC",
                    Mensaje = ex.Message,
                    Metodo = "ResumenNotificaciones",
                    Parametros = "{idEmpresa:" + idEmpresa + "}",
                    Usuario = usuarioLog.idUsuario.ToString()
                };

                new LogSessionesDAC(usuarioLog).GrabaLogLocalErrores(log);
                throw new Exception(ex.Message);
            }
        }

        public bool InsertaHdPerNotificacion(int codNotificacion, int hdPer, int codCont, int CodDoc, string TipFirma)
        {
            try
            {
                using (SqlConnection conexion = new SqlConnection(strConexion))
                {
                    conexion.Open();
                    using (SqlCommand command = new SqlCommand { Connection = conexion, CommandType = CommandType.StoredProcedure, CommandText = "Insert_HDPer_Notificacion", CommandTimeout = 10 })
                    {

                        command.Parameters.AddWithValue("@pnhcodpno_ta", codNotificacion);
                        command.Parameters.AddWithValue("@pnhchdper_ta", hdPer);
                        command.Parameters.AddWithValue("@usuario", usuarioLog.idUsuario);
                        command.Parameters.AddWithValue("@pnhcodcon_ta", codCont);
                        command.Parameters.AddWithValue("@pnhcoddoc_ta", CodDoc);
                        command.Parameters.AddWithValue("@pnhtipfir_re", TipFirma);
                        command.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                LogLocalBE log = new LogLocalBE()
                {
                    Fecha = DateTime.Now,
                    Funcion = "NotificacionesDAC",
                    Mensaje = ex.Message,
                    Metodo = "InsertaHdPerNotificacion",
                    Parametros = JsonConvert.SerializeObject("{codNotificacion:" + codNotificacion +
                    ",hdPer:" + hdPer +
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
                using (SqlConnection conexion = new SqlConnection(strConexion))
                {
                    conexion.Open();
                    using (SqlCommand command = new SqlCommand { Connection = conexion, CommandType = CommandType.StoredProcedure, CommandText = "Firma_HDPer_Notificacion", CommandTimeout = 10 })
                    {

                        command.Parameters.AddWithValue("@pnhcodpno_ta", codNotificacion);
                        command.Parameters.AddWithValue("@pnhcodcon_ta", CodContrato);
                        command.Parameters.AddWithValue("@usuariofirma", usuarioLog.idUsuario);
                        command.Parameters.AddWithValue("@pnhtokfir_cr", TokenFirma);
                        command.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                LogLocalBE log = new LogLocalBE()
                {
                    Fecha = DateTime.Now,
                    Funcion = "NotificacionesDAC",
                    Mensaje = ex.Message,
                    Metodo = "InsertDestinatarioNoficiacion",
                    Parametros = JsonConvert.SerializeObject("{codNotificacion:" + codNotificacion +
                    ",codPersona:" + codPersona +
                    ",CodContrato:" + CodContrato +
                    ",usuario:" + usuarioLog.idUsuario +
                    ",TokenFirma:" + TokenFirma + "}"),
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
                using (SqlConnection conexion = new SqlConnection(strConexion))
                {
                    conexion.Open();
                    using (SqlCommand command = new SqlCommand { Connection = conexion, CommandType = CommandType.StoredProcedure, CommandText = "Valida_NotificacionPendiente", CommandTimeout = 10 })
                    {

                        command.Parameters.AddWithValue("@CodContrato", CodContrato);
                        command.Parameters.AddWithValue("@CodPersona", codPersona);
                        command.Parameters.AddWithValue("@TipoDocPend", TipoDocPen);
                        using (SqlDataAdapter Adapter = new SqlDataAdapter(command))
                        {
                            DataTable dt = new DataTable();
                            Adapter.Fill(dt);
                            return dt;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogLocalBE log = new LogLocalBE()
                {
                    Fecha = DateTime.Now,
                    Funcion = "NotificacionesDAC",
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
                using (SqlConnection conexion = new SqlConnection(strConexion))
                {
                    conexion.Open();
                    using (SqlCommand command = new SqlCommand { Connection = conexion, CommandType = CommandType.StoredProcedure, CommandText = "Anula_NotificacionPendiente", CommandTimeout = 10 })
                    {

                        command.Parameters.AddWithValue("@CodContrato", CodContrato);
                        command.Parameters.AddWithValue("@CodPersona", codPersona);
                        command.Parameters.AddWithValue("@TipoDocPend", TipoDocPen);
                        command.Parameters.AddWithValue("@usuaccion", usuarioLog.idUsuario);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                LogLocalBE log = new LogLocalBE()
                {
                    Fecha = DateTime.Now,
                    Funcion = "NotificacionesDAC",
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
