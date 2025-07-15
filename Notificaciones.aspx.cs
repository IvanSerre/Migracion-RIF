using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using RIF_Logic_Access.BC;
using RIF_Logic_Access.BE;
using System;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.Services;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace RIF_Front
{
    public partial class Notificaciones : System.Web.UI.Page
    {
        UsuarioBE usuario = new UsuarioBE();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["Usuario"] != null)
                usuario = ((UsuarioBE)Session["Usuario"]);
            else
            {
                Response.Write("<script>location.href = 'Login.aspx'</script>");
                Response.End();
            }

            DataTable dtTipoNot = new NotificacionesBC(usuario).RescataListadoCatalogoNotificaciones();

            //cmbTipoNotificacion.DataSource = dtTipoNot;
            //cmbTipoNotificacion.DataBind();

            cmbEstadoNotificacion.DataSource = new DescripcionReglaBC(usuario).RescataReglas(new ReglaBE() { NombreRegla = "EstadoNotificacion" }, "Estados");
            cmbEstadoNotificacion.DataBind();

            cmbTipoNotificacionFilter.DataSource = dtTipoNot;
            cmbTipoNotificacionFilter.DataBind();

            //cmbPara.DataSource = new ListadoBC(usuario).ListadoEmpleados(((EmpresaLitleBE)HttpContext.Current.Session["EmpSel"]).idEmpresa);
            //cmbPara.DataBind();

            //cmbTipoDoc.DataSource = new DescripcionReglaBC(usuario).RescataReglas(new ReglaBE() { NombreRegla = "DocumentosFirma" }, "Tipo de Documento");
            //cmbTipoDoc.DataBind();

            using (DataSet dsResumen = new NotificacionesBC(usuario).ResumenNotificaciones(((EmpresaLitleBE)HttpContext.Current.Session["EmpSel"]).idEmpresa)) {

                int contadorStyle = -1;
                foreach (DataRow fila in dsResumen.Tables[0].Rows)
                {
                    contadorStyle = contadorStyle == 2 ? 0 : contadorStyle + 1;
                    string[] Styles = { "success", "primary", "secondary" };

                    HtmlGenericControl divcol = new HtmlGenericControl("DIV");
                    divcol.Attributes.Add("class", "col-12");
                    divcol.Attributes.Add("onclick", "GrillaNotificacion('" + fila["TipoNotificacion"].ToString() + "')");

                    HtmlGenericControl divcard = new HtmlGenericControl("DIV");
                    divcard.Attributes.Add("class", "card cursor");
                    divcol.Controls.Add(divcard);

                    HtmlGenericControl divbody = new HtmlGenericControl("DIV");
                    divbody.Attributes.Add("class", "card-body");
                    divcard.Controls.Add(divbody);

                    HtmlGenericControl divflex = new HtmlGenericControl("DIV");
                    divflex.Attributes.Add("class", "d-flex justify-content-between");
                    divbody.Controls.Add(divflex);

                    HtmlGenericControl div = new HtmlGenericControl("DIV");
                    divflex.Controls.Add(div);

                    HtmlGenericControl h5 = new HtmlGenericControl("H5");
                    div.Controls.Add(h5);

                    HtmlGenericControl b = new HtmlGenericControl("b") { InnerText = fila["Notificacion"].ToString() + " (" + fila["CantidadNotificaciones"].ToString() + ")" };
                    b.Attributes.Add("class", "textComplementario");
                    h5.Controls.Add(b);

                    HtmlGenericControl h6 = new HtmlGenericControl("H6") { InnerText = "Pendientes" };
                    h6.Attributes.Add("class", "text-muted");
                    div.Controls.Add(h6);


                    HtmlGenericControl h3 = new HtmlGenericControl("H3") { InnerText = (Int32.Parse(fila["NotificacionesEnviadas"].ToString()) - Int32.Parse(fila["NotificacionesRespondidas"].ToString())).ToString() };
                    h3.Attributes.Add("class", "text-" + Styles[contadorStyle] + " fw-bold m-3");
                    divflex.Controls.Add(h3);

                    HtmlGenericControl divprogress = new HtmlGenericControl("DIV");
                    divprogress.Attributes.Add("class", "progress progress-sm");
                    divbody.Controls.Add(divprogress);

                    string Procentaje="0";
                    try
                    {
                        Procentaje = Int32.Parse(fila["NotificacionesRespondidas"].ToString()) == 0 ? "100" : (Int32.Parse(fila["NotificacionesRespondidas"].ToString()) * 100 / Int32.Parse(fila["NotificacionesEnviadas"].ToString())).ToString();
                    }
                    catch
                    {
                        Procentaje = "0";
                    }

                    HtmlGenericControl divprogressbar = new HtmlGenericControl("DIV");
                    divprogressbar.Attributes.Add("class", "progress-bar bg-" + Styles[contadorStyle]);
                    divprogressbar.Attributes.Add("style", "width: " + Procentaje + "% !important");
                    divprogressbar.Attributes.Add("role", "progressbar");
                    divprogressbar.Attributes.Add("aria-valuenow", Procentaje);
                    divprogressbar.Attributes.Add("aria-valuemin", "0");
                    divprogressbar.Attributes.Add("aria-valuemax", "100");
                    divprogress.Controls.Add(divprogressbar);

                    HtmlGenericControl divflex2 = new HtmlGenericControl("DIV");
                    divflex2.Attributes.Add("class", "d-flex justify-content-between mt-2");
                    divbody.Controls.Add(divflex2);

                    HtmlGenericControl p1 = new HtmlGenericControl("p") { InnerText = "Porcentaje Pendiente" };
                    p1.Attributes.Add("class", "text-muted mb-0");
                    divflex2.Controls.Add(p1);

                    HtmlGenericControl p2 = new HtmlGenericControl("p") { InnerText = Procentaje + "%" };
                    p2.Attributes.Add("class", "text-muted mb-0");
                    divflex2.Controls.Add(p2);

                    CalugasResumen.Controls.Add(divcol);

                    switch(fila["TipoNotificacion"].ToString())
                    {
                        case "SOFI":
                            SOFIPEN.InnerText = (Int32.Parse(fila["CantidadNotificaciones"].ToString().PadLeft(1, '0')) - Int32.Parse(fila["NotificacionesRespondidas"].ToString().PadLeft(1, '0'))).ToString();
                            SOFITOT.InnerText = fila["CantidadNotificaciones"].ToString().PadLeft(1, '0');
                            break;
                        case "SODO":
                            SODOPEN.InnerText = (Int32.Parse(fila["CantidadNotificaciones"].ToString().PadLeft(1, '0')) - Int32.Parse(fila["NotificacionesRespondidas"].ToString().PadLeft(1, '0'))).ToString();
                            SODOTOT.InnerText = fila["CantidadNotificaciones"].ToString().PadLeft(1, '0');
                            break;
                        case "MEIN":
                            MEINTOT.InnerText = fila["NotificacionesEnviadas"].ToString().PadLeft(1, '0');
                            break;
                    }
                }

            }
        }


        [WebMethod(enableSession: true)]
        public static string GrabaNotificacion(NotificacionBE Notificacion)
        {
            try
            {
                if (HttpContext.Current.Session.Count == 0)
                    return JsonConvert.SerializeObject(new EmpresaBE() { EstadoTransaccion = new ResultadoRetornoBE() { codResultado = 501, Estado = EstadoTransaccion.NOSESSION, Mensaje = "Su sesión ha expirado, autentiquese nuevamente" } });
                var usuario = (UsuarioBE)HttpContext.Current.Session["usuario"];

                Notificacion.CodEntidad = ((EmpresaLitleBE)HttpContext.Current.Session["EmpSel"]).idEmpresa;
                Notificacion.UsuarioCreador = usuario.idUsuario;

                return JsonConvert.SerializeObject(new NotificacionesBC(usuario).SetNotificacion(Notificacion,true, null));
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new EmpresaBE() { EstadoTransaccion = new ResultadoRetornoBE() { codResultado = 500, Estado = EstadoTransaccion.ERROR, Mensaje = ex.Message } });
            }
        }

        [WebMethod(enableSession: true)]
        public static string AnularNotificacion(int idNotificacion)
        {
            try
            {
                if (HttpContext.Current.Session.Count == 0)
                    return JsonConvert.SerializeObject(new EmpresaBE() { EstadoTransaccion = new ResultadoRetornoBE() { codResultado = 501, Estado = EstadoTransaccion.NOSESSION, Mensaje = "Su sesión ha expirado, autentiquese nuevamente" } });
                var usuario = (UsuarioBE)HttpContext.Current.Session["usuario"];
                if (new CargosBC(usuario).Baja_Cargos(idNotificacion, Int32.Parse(HttpContext.Current.Session["idEmpEnrolDet"].ToString())))
                return JsonConvert.SerializeObject(new EmpresaBE() { EstadoTransaccion = new ResultadoRetornoBE() { codResultado = 200, Estado = EstadoTransaccion.OK, Mensaje = "OK" } });
                else
                return JsonConvert.SerializeObject(new EmpresaBE() { EstadoTransaccion = new ResultadoRetornoBE() { codResultado = 500, Estado = EstadoTransaccion.ERROR, Mensaje = "Ocurrió un error al dar la baja del cargo" } });
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new EmpresaBE() { EstadoTransaccion = new ResultadoRetornoBE() { codResultado = 500, Estado = EstadoTransaccion.ERROR, Mensaje = ex.Message } });
            }
        }


        [WebMethod(enableSession: true)]
        public static string ObtieneNotificaciones(int idNotificacion, string tipoNotificacion)
        {
            try
            {
                if (HttpContext.Current.Session.Count == 0)
                    return JsonConvert.SerializeObject(new EmpresaBE() { EstadoTransaccion = new ResultadoRetornoBE() { codResultado = 501, Estado = EstadoTransaccion.NOSESSION, Mensaje = "Su sesión ha expirado, autentiquese nuevamente" } });
                var usuario = (UsuarioBE)HttpContext.Current.Session["usuario"];

                return JsonConvert.SerializeObject(new NotificacionesBC(usuario).RescataNotificaciones(idNotificacion, tipoNotificacion, ((EmpresaLitleBE)HttpContext.Current.Session["EmpSel"]).idEmpresa));
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new ResultadoRetornoBE() { codResultado = 500, Estado = EstadoTransaccion.ERROR, Mensaje = ex.Message });
            }
        }

        [WebMethod(enableSession: true)]
        public static string ObtieneCatalogoTipoNot(string idCatNot)
        {
            try
            {
                if (HttpContext.Current.Session.Count == 0)
                    return JsonConvert.SerializeObject(new EmpresaBE() { EstadoTransaccion = new ResultadoRetornoBE() { codResultado = 501, Estado = EstadoTransaccion.NOSESSION, Mensaje = "Su sesión ha expirado, autentiquese nuevamente" } });
                var usuario = (UsuarioBE)HttpContext.Current.Session["usuario"];

                return JsonConvert.SerializeObject(new NotificacionesBC(usuario).RescataCatalogoNotificaciones(idCatNot));
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new EmpresaBE() { EstadoTransaccion = new ResultadoRetornoBE() { codResultado = 500, Estado = EstadoTransaccion.ERROR, Mensaje = ex.Message } });
            }
        }
        [WebMethod(enableSession: true)]
        public static string AnulaNotificacion(int idNotificacion)
        {
            try
            {
                if (HttpContext.Current.Session.Count == 0)
                    return JsonConvert.SerializeObject(new EmpresaBE() { EstadoTransaccion = new ResultadoRetornoBE() { codResultado = 501, Estado = EstadoTransaccion.NOSESSION, Mensaje = "Su sesión ha expirado, autentiquese nuevamente" } });
                var usuario = (UsuarioBE)HttpContext.Current.Session["usuario"];
                if (new NotificacionesBC(usuario).AnularNotificacion(idNotificacion, ((EmpresaLitleBE)HttpContext.Current.Session["EmpSel"]).idEmpresa))
                    return JsonConvert.SerializeObject(new EmpresaBE() { EstadoTransaccion = new ResultadoRetornoBE() { codResultado = 200, Estado = EstadoTransaccion.OK, Mensaje = "OK" } });
                else
                    return JsonConvert.SerializeObject(new EmpresaBE() { EstadoTransaccion = new ResultadoRetornoBE() { codResultado = 500, Estado = EstadoTransaccion.ERROR, Mensaje = "Ocurrió un error al anular la notificación" } });
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new EmpresaBE() { EstadoTransaccion = new ResultadoRetornoBE() { codResultado = 500, Estado = EstadoTransaccion.ERROR, Mensaje = ex.Message } });
            }
        }

        [WebMethod(enableSession: true)]
        public static string EstadosFlujoNotificacion(int idNotificacion, int idPersona)
        {
            try
            {
                if (HttpContext.Current.Session.Count == 0)
                    return JsonConvert.SerializeObject(new EmpresaBE() { EstadoTransaccion = new ResultadoRetornoBE() { codResultado = 501, Estado = EstadoTransaccion.NOSESSION, Mensaje = "Su sesión ha expirado, autentiquese nuevamente" } });
                var usuario = (UsuarioBE)HttpContext.Current.Session["usuario"];

                return JsonConvert.SerializeObject(new NotificacionesBC(usuario).EstadosFlujoNotificacion(idNotificacion, idPersona,((EmpresaLitleBE)HttpContext.Current.Session["EmpSel"]).idEmpresa));
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new ResultadoRetornoBE() { codResultado = 500, Estado = EstadoTransaccion.ERROR, Mensaje = ex.Message });
            }
        }

        [WebMethod(enableSession: true)]
        public static string NotificacionesxPersona(int idNotificacion, int Persona)
        {
            try
            {
                if (HttpContext.Current.Session.Count == 0)
                    return JsonConvert.SerializeObject(new ResultadoRetornoBE() { codResultado = 501, Estado = EstadoTransaccion.NOSESSION, Mensaje = "Su sesión ha expirado, autentiquese nuevamente" });
                var usuario = (UsuarioBE)HttpContext.Current.Session["usuario"];

                return JsonConvert.SerializeObject(new NotificacionesBC(usuario).NotificacionesxPersona(idNotificacion, Persona, ((EmpresaLitleBE)HttpContext.Current.Session["EmpSel"]).idEmpresa, false));
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new EmpresaBE() { EstadoTransaccion = new ResultadoRetornoBE() { codResultado = 500, Estado = EstadoTransaccion.ERROR, Mensaje = ex.Message } });
            }
        }

        [WebMethod(enableSession: true)]
        public static string ReenviaNotificacion(int idNotificacion)
        {
            try
            {
                if (HttpContext.Current.Session.Count == 0)
                    return JsonConvert.SerializeObject(new EmpresaBE() { EstadoTransaccion = new ResultadoRetornoBE() { codResultado = 501, Estado = EstadoTransaccion.NOSESSION, Mensaje = "Su sesión ha expirado, autentiquese nuevamente" } });
                var usuario = (UsuarioBE)HttpContext.Current.Session["usuario"];
                return JsonConvert.SerializeObject(new NotificacionesBC(usuario).ReenviaNotificacion(idNotificacion, ((EmpresaLitleBE)HttpContext.Current.Session["EmpSel"]).idEmpresa));
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new EmpresaBE() { EstadoTransaccion = new ResultadoRetornoBE() { codResultado = 500, Estado = EstadoTransaccion.ERROR, Mensaje = ex.Message } });
            }
        }
    }
}