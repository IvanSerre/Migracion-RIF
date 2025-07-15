using System;
using System.Collections.Generic;


namespace RIF_Logic_Access.BE
{
    public class NotificacionBE
    {
        public int CodNotificacion { get; set; }
        public string NombreNotificacion { get; set; }
        public int CodEntidad { get; set; }
        public string TipoNot { get; set; }
        public string MensajeEntrada { get; set; }
        public DateTime FechaNot { get; set; }
        public DateTime FechaCreacion { get; set; }
        public int UsuarioCreador { get; set; }
        public string NombreDocAdjunto { get; set; }
        public DocumentoBE DocumentoAdjunto { get; set; }
        public string TomaConocimiento { get; set; }
        public string MensajeRespuesta { get; set; }
        public string EstadoNotificacion { get; set; }
        public List<DestinatariosBE> Destinatarios { get; set; }
        public List<string> Antecedentes { get; set; }
        public ResultadoRetornoBE EstadoTransaccion { get; set; }
        public List<int> HDPerAsociados { get; set; }
    }

    public class DestinatariosBE
    {
        public int CodNotificacion { get; set; }
        public int CodDestinatario { get; set; }
        public string TipoDestinatario { get; set; }
    }

    public class PersonaNotificacionBE
    {
        public int idPerNotif { get; set; }
        public int idEntidad { get; set; }
        public int CodigoNotificacion { get; set; }
        public int CodigoPersona { get; set; }
        public string Estado { get; set; }
        public string Resultado { get; set; }
        public int UsuarioAccion { get; set; }
        public string DescripcionResultado { get; set; }
        public DateTime FechaLectura { get; set; }
        public DateTime FechaRespuesta { get; set; }
        public string TextoRespuesta { get; set; }
        public string ArchivoRespuesta { get; set; }
        public List<string> Antecedentes { get; set; }
        public List<int> HDPerAsociados { get; set; }


    }
}

