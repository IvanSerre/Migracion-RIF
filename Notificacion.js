MensajeAdjunto = false;
var Base64 = '';
$(document).ready(function () {
    $(".cmbPara").select2();
    $("#AntecedentesDocs").tagsinput({ tagClass: 'big' });

    $('button').click(function (event) { event.stopPropagation(); });
    $("#cmbTipoNotificacion").on("change", function () {
        const idNot = $(this).val();

        if (idNot == 'SODO') {
            $("#contentDivSol").fadeIn("slow");
        }
        else {
            $("#contentDivSol").fadeOut("slow");
        }

        $.ajax({
            type: "POST",
            data: JSON.stringify({ idCatNot: idNot }),
            contentType: "application/json; charset=utf-8",
            url: "Notificaciones.aspx/ObtieneCatalogoTipoNot",
            success: function (data) {

                result = eval('(' + data.d + ')')
                if (result.Estado == 3) notSession();
                if (result.Estado == undefined || result.Estado == 1) {
                    if (result[0].cnoadjent_fl == 'S')
                        $("#DocCar").prop("disabled", false);
                    else
                        $("#DocCar").prop("disabled", true);

                    if (result[0].cnomenent_fl == 'S') {
                        $("#txtMsj").slideDown('fast');
                        MensajeAdjunto = true;
                    }
                    else {
                        $("#txtMsj").slideUp('fast');
                        $("#txtMsj").val('');
                        MensajeAdjunto = false;

                    }
                    if (result[0].cnofirele_fl == 'S') {
                        $(".requiereFirma").fadeIn('fast');
                        $("#PanelcmbTipoDoc").slideDown('fast');
                    }
                    else {
                        $(".requiereFirma").fadeOut('fast');
                        $("#PanelcmbTipoDoc").slideUp('fast');
                    }
                }
                else {
                    swal("!Error en el tipo de Notificación!", 'Ocurrió un error al rescatar las condiciones del tipo de notificación', {
                        icon: "error",
                        buttons: {
                            confirm: {
                                className: 'btn btn-danger'
                            }
                        },
                    });
                }
            },
            error: function (xhr, ajaxOptions, thrownError) {
                swal("!Error en el tipo de Notificación!", 'Ocurrió un error al rescatar las condiciones del tipo de notificación', {
                    icon: "error",
                    buttons: {
                        confirm: {
                            className: 'btn btn-danger'
                        }
                    },
                });
            },
        });
    });
    $("#SetNotifica").on("click", function () {
        debugger;
        $('.message-error').remove();
        $('.input-error').removeClass("input-error");

        if ($("#txtNombre").val() == "")
            return MarcaErroCampo("Ingresa el nombre de la Notificación", $("#txtNombre"));
        if ($("#cmbTipoNotificacion").val() == "" || $("#cmbTipoNotificacion").val() == "0" || $("#cmbTipoNotificacion").val() == null)
            return MarcaErroCampo("Indica el tipo de Notificación", $("#cmbTipoNotificacion"));
        if ($('#txtFecInicioNot.input_datepicker').datepicker('getDate').val() == "")
            return MarcaErroCampo("La fecha de envío de la notificación es una dato necesario", $("#txtFecInicioNot"));
        if ($("#cmbPara").val() == "" || $("#cmbPara").val() == "0" || $("#cmbPara").val() == null)
            return MarcaErroCampo("", $("#cmbPara"));
        if ($("#txtMensaje").val() == "" && MensajeAdjunto)
            return MarcaErroCampo("La notificación requiere un mensaje adjunto", $("#txtMensaje"));
        if ($("#AntecedentesDocs").val() == "" && $("#cmbTipoNotificacion").val() == "SODO")
            return MarcaErroCampo("Ingresa los documentos solicitados", $("#AntecedentesDocs"));
        if ($("#cmbTipoDoc").val() == "" && $("#cmbTipoNotificacion").val() == "SOFI") 
            return MarcaErroCampo("Debes indicar el tipo de documento al cual solicitas firma", $("#cmbTipoDoc"));

        if ($("#fupload")[0].files.length == 0 && $("#cmbTipoNotificacion").val() == "SOFI") {
            swal("Debes adjuntar el documento que requiere de Firma", "Este tipo de notificación requiere un documento adjunto", { icon: "error", buttons: { confirm: { className: 'btn btn-danger' } }, });
            return false;
        }

        var fecini = $('#txtFecInicioNot.input_datepicker').datepicker('getDate').val().split('-');

        var DocAdj = {};
        if ($("#cmbTipoNotificacion").val() == "SOFI") {
            var datosFiles = Base64.split(';base64,');
            DocAdj = {
                Documento: datosFiles[1],
                NombreDoc: $("#fupload")[0].files[0].name,
                ContentType: datosFiles[0].split(":")[1],
                TipoDoc: $("#cmbTipoDoc").val()
            }
        }

        var Data = {
            CodNotificacion: $("#refNotif").val(),
            NombreNotificacion: $("#txtNombre").val(),
            TipoNot: $("#cmbTipoNotificacion").val(),
            FechaNot: new Date(parseInt(fecini[2]), parseInt(fecini[1]) - 1, parseInt(fecini[0])),
            Destinatarios: [],
            MensajeEntrada: $("#txtMensaje").val(),
            MensajeRespuesta: $("#chkMsjRes").prop("checked") ? 'S' : 'N',
            TomaConocimiento: $("#chkTomCon").prop("checked") ? 'S' : 'N',
            Antecedentes: $("#AntecedentesDocs").tagsinput('items'),
            DocumentoAdjunto: DocAdj
        }
        $.each($("#cmbPara").val(), function (i, val) {
            Data.Destinatarios.push({ CodDestinatario: val, TipoDestinatario: 'PE' });
        });

        $.ajax({
            type: "POST",
            data: JSON.stringify({ Notificacion: Data }),
            contentType: "application/json; charset=utf-8",
            url: "Notificaciones.aspx/GrabaNotificacion",
            success: function (data) {
                result = eval('(' + data.d + ')')
                if (result.EstadoTransaccion.Estado == 3) notSession();
                if (result.EstadoTransaccion.Estado == 1) {
                    if ($("#refNotif").val() == "0") {
                        swal("Grabación exitosa!", 'La Notificación fue ingresada con exito', {
                            icon: "success",
                            buttons: {
                                confirm: {
                                    className: 'btn btn-success'
                                }
                            },
                        }).then(() => {
                            $("#ModalNotificacion").modal('hide');
                            LoadPage('/NOTIFICACIONES');
                            LimpiaNotificacion();
                        });
                    }
                    else {
                        swal("Actualización exitosa!", 'La Notificación fue actualizada con exito', {
                            icon: "success",
                            buttons: {
                                confirm: {
                                    className: 'btn btn-success'
                                }
                            },
                        }).then(() => {
                            $("#ModalNotificacion").modal('hide');
                            LoadPage('/NOTIFICACIONES');
                            LimpiaNotificacion();
                        })
                    }
                }
                else
                    swal("Ocurrió un Error al grabar la notificación", result.EstadoTransaccion.Mensaje, {
                        icon: "error",
                        buttons: {
                            confirm: {
                                className: 'btn btn-danger'
                            }
                        },
                    });
            },
            error: function (xhr, ajaxOptions, thrownError) {
                swal("Ocurrió un Error al grabar la notificación", eval('(' + xhr.responseText + ')').Message, {
                    icon: "error",
                    buttons: {
                        confirm: {
                            className: 'btn btn-danger'
                        }
                    },
                });
            },
        });
    });
    $('#ModalNotificacion').on('shown.bs.modal', function () {
        $("#txtNombre").focus();
    });
    $("#BajaNot").unbind();
    $("#BajaNot").on('click', function () {
        BajaNot($("#refNotif").val());
    });
    $("#SendNotifica").unbind();
    $("#SendNotifica").on('click', function () {
        ResendNot($("#refNotif").val());
    });
    $("#DocCar").on("click", function () {
        $("#fupload").click();
    });

    $("#fupload").on("change", function () {
        getBase64($("#fupload")[0].files[0]);
    });

});
function DetalleNot(id) {
    $("#ModalNotificacion").modal('show');
    $.ajax({
        type: "POST",
        data: JSON.stringify({ idNotificacion: id, tipoNotificacion: null }),

        contentType: "application/json; charset=utf-8",
        url: "Notificaciones.aspx/ObtieneNotificaciones",
        success: function (data) {
            result = eval('(' + data.d + ')')
            if (result.Estado == 3) notSession();
            if (result.Estado == undefined || result.Estado == 1) {
                $("#Loading").fadeOut("slow");
                $("#refNotif").val(result.Resultado[0].idNotPersona);
                $("#txtNombre").html(result.Resultado[0].nombre);
                $("#cmbTipoNotificacion").html(result.Resultado[0].tipo);
                $("#txtFecInicioNot").html(moment(result.Resultado[0].fechanoti).format("DD-MM-YYYY"));
                $("#txtMensaje").html(result.Resultado[0].mensaje);
                $("#cmbTipoNotificacion").prop("disabled", true);
                $("#AntecedentesDocs").prop("disabled", true);
                $("#txtPara").html(result.Resultado[0].nombrePersona);

                var flowNoti = JSON.parse(result.Resultado[0].flowNoti);
                var flujo = "<div class='timelineH' style='margin:0px auto;width:" + (flowNoti.length > 3 ? "400px" : "300px") + " !important'>";
                
                $.each(flowNoti, function (i, val) {
                    flujo = flujo +
                        "<div class='timeLineItem'>" +
                        "<div class='timeline-badge " + (val.estadoExiste == 0 ? "" : "check") + "'>" +
                        "<i class='fas fa-check'></i>" +
                        "</div>" +
                        "<div class='leyenda'>" + val.ecngloest_cr + "</div>" +
                        "</div>" +
                        "<div class='lineTime'>" +
                        (i < flowNoti.length - 1 ? "<i class='fas fa-arrow-right'></i>" : "") +
                        "</div>"
                });
                flujo = flujo + "</div>";

                $("#flujoModal").html(flujo);



                // Docs

                $.ajax({
                    type: "POST",
                    data: JSON.stringify({ idNotificacion: result.Resultado[0].idNotPersona, Persona: result.Resultado[0].personaCod }),
                    contentType: "application/json; charset=utf-8",
                    url: "Notificaciones.aspx/NotificacionesxPersona",
                    success: function (data) {
                        result = eval('(' + data.d + ')')
                        if (result.EstadoTransaccion?.Estado == 3) notSession();
                        if (result.EstadoTransaccion == undefined || result.EstadoTransaccion.Estado == 1) {
                            $("#CabeceraTipoNot").text(result.Table[0].tipo)
                            $("#nombreNotificacion").text(result.Table[0].nombre)
                            $("#mensajeNotificacion").text(result.Table[0].mensaje);
                            $("#key").val(result.Table[0].idNotPer);
                            $("#tipDoc").val(result.Table[0].TipDoc);
                            if (result.Table[0].idtipo == 'SOFI') {
                                $('#tblDocsSol').hide();
                                $('#ContentSolFir').show();
                                $('#TblDocsFirmas').DataTable({
                                    ajax: {
                                        'data': function (d) {
                                            return "{idNotificacion:" + result.Table[0].idNotPer + ",TipoDoc:'" + result.Table[0].TipDoc + "'}";
                                        },
                                        url: "Personal_Documentos.aspx/VerDocListFirma",
                                        method: "POST",
                                        dataType: 'json',
                                        contentType: "application/json; charset=utf-8",
                                        dataSrc: function (data) {
                                            return $.parseJSON(data.d);
                                        }
                                    },
                                    bPaginate: false,
                                    bFilter: false,
                                    bInfo: false,
                                    destroy: true,
                                    fnDrawCallback: function () {
                                        $("#TblDocsFirmas thead").remove();
                                    },
                                    columns: [
                                        {
                                            data: 'Nombre', title: 'Nombre', className: 'head-center content-center', render: function (data, type, row) {
                                                return '<div onclick="VerDoc(' + row.id + ',\'' + result.Table[0].idtipo + '\')">' + data + '</div>';
                                            }
                                        }
                                    ],
                                });
                            }
                            else {
                                if (result.Table[0].idtipo == 'SODO') {
                                    $('#tblDocsSol').show();
                                    $('#ContentSolFir').hide();
                                    $('#tblDocsSol').DataTable({
                                        data: result.Table2,
                                        bPaginate: false,
                                        bFilter: false,
                                        bInfo: false,
                                        destroy: true,
                                        columns: [
                                            { data: 'nombre', title: 'Nombre', className: 'head-center content-center' },
                                            {
                                                data: 'fecenv', title: 'Fecha de Carga', className: 'head-center content-center', render: function (data, type, row) {
                                                    if (type === "sort" || type === "type") {
                                                        return data;
                                                    }
                                                    if (data != null)
                                                        return moment(data).format("DD-MM-YYYY HH:mm A");
                                                    else
                                                        return '-';
                                                }
                                            },
                                            {
                                                className: 'head-center content-center',
                                                orderable: false,
                                                data: null,
                                                defaultContent: '',
                                                title: 'Accion',
                                                render: function (data, type, row) {
                                                    if (data.fecenv)
                                                        return '<div class="iconDocs" title="Ver Documento"  onclick=javaScript:VerDoc(' + data.id + ')><i class="flaticon-file large"></i> <h5>Ver</h5></div>';
                                                    else
                                                        return '<div class="iconDocs" title="Ver Documento"></div>';
                                                }
                                            }
                                        ],
                                    });
                                }
                                else {
                                    $('#tblDocsSol').show();
                                    $('#ContentSolFir').hide();
                                }
                            }
                        }
                        else
                            swal("¡Ocurrió un Error al cargar la información!", result.EstadoTransaccion.Mensaje, {
                                icon: "error",
                                buttons: {
                                    confirm: {
                                        className: 'btn btn-danger'
                                    }
                                },
                            });
                    },
                    error: function (xhr, ajaxOptions, thrownError) {
                        swal("¡Ocurrió un Error al cargar la información!", eval('(' + xhr.responseText + ')').Message, {
                            icon: "error",
                            buttons: {
                                confirm: {
                                    className: 'btn btn-danger'
                                }
                            },
                        });
                    },
                });


            }
            else {
                swal("No se pudo rescata la notificación!", result.Mensaje, {
                    icon: "error",
                    buttons: {
                        confirm: {
                            className: 'btn btn-danger'
                        }
                    },
                });
            }
        },
        error: function (xhr, ajaxOptions, thrownError) {
            swal("No se pudo rescata la notificación!", eval('(' + xhr.responseText + ')').Message, {
                icon: "error",
                buttons: {
                    confirm: {
                        className: 'btn btn-danger'
                    }
                },
            });
            $("#Loading").fadeOut("slow");
        },
    });
}
function LimpiaNotificacion() {
    $("#contentDivSol").fadeOut("slow", function () {
        $("#txtMsj").slideUp('fast', function () {
            $("#refNotif").val('0');
            $("#txtNombre").val('');
            $("#cmbTipoNotificacion").val('0');
            $("#txtFecInicioNot.input_datepicker").datepicker("update", '');
            $("#txtMensaje").val('');
            $("#chkTomCon").prop("checked", false);
            $("#chkMsjRes").prop("checked", false);
            $("#cmbPara").val([]);
            $("#cmbPara").select2();
            $("#cmbPara").prop("disabled", false);
            $("#cmbTipoNotificacion").prop("disabled", false);
            $("#AntecedentesDocs").tagsinput('removeAll');
        });
    });
    MensajeAdjunto = false;
}
function BajaNot(Not) {
    swal({
        title: 'Anular Notificación',
        text: "¿Está seguro que desea anular la notificación?",
        type: 'success',
        buttons: {
            confirm: {
                text: 'Si, anular',
                className: 'btn btn-success'
            },
            cancel: {
                text: 'Cancelar',
                visible: true,
                className: 'btn btn-warning'
            }
        }
    }).then((Cofirmar) => {
        if (Cofirmar) {
            $.ajax({
                type: "POST",
                data: JSON.stringify({ idNotificacion: Not }),
                contentType: "application/json; charset=utf-8",
                url: "Notificaciones.aspx/AnulaNotificacion",
                success: function (data) {
                    result = eval('(' + data.d + ')')
                    if (result.EstadoTransaccion.Estado == 3) notSession();
                    if (result.EstadoTransaccion.Estado != 1) {
                        swal("Error al anular la notificación", result.EstadoTransaccion.Mensaje, {
                            icon: "error",
                            buttons: {
                                confirm: {
                                    className: 'btn btn-danger'
                                }
                            },
                        });
                    }
                    else {
                        swal("Anulación Exitosa", "Notificación anulada exitosamente", {
                            icon: "success",
                            buttons: {
                                confirm: {
                                    className: 'btn btn-success'
                                }
                            },
                        }).then(() => {
                            $("#ModalNotificacion").modal('hide');
                            GrillaNotificacion(null);
                        });
                    }
                },
                error: function (xhr, ajaxOptions, thrownError) {
                    swal("Error al anular la notificación", eval('(' + xhr.responseText + ')').Message, {
                        icon: "error",
                        buttons: {
                            confirm: {
                                className: 'btn btn-danger'
                            }
                        },
                    });
                },
            });
        } else {
            swal.close();
        }
    });
}
function ResendNot(Not) {
    $.ajax({
        type: "POST",
        data: JSON.stringify({ idNotificacion: Not }),
        contentType: "application/json; charset=utf-8",
        url: "Notificaciones.aspx/ReenviaNotificacion",
        success: function (data) {
            result = eval('(' + data.d + ')')
            if (result.Estado == 3) notSession();
            if (result.Estado != 1) {
                swal("Error al reenviar la notificación", result.Mensaje, {
                    icon: "error",
                    buttons: {
                        confirm: {
                            className: 'btn btn-danger'
                        }
                    },
                });
            }
            else {
                swal("Reenvío Exitoso", "Se ha reenviado la notificación de forma exitosa a: " + result.Resultado, {
                    icon: "success",
                    buttons: {
                        confirm: {
                            className: 'btn btn-success'
                        }
                    },
                }).then(() => {});
            }
        },
        error: function (xhr, ajaxOptions, thrownError) {
            swal("Error al reenviar la notificación", eval('(' + xhr.responseText + ')').Message, {
                icon: "error",
                buttons: {
                    confirm: {
                        className: 'btn btn-danger'
                    }
                },
            });
        },
    });
}
var FlujoEstados = function (idNotificacion, idPersona) {
    return new Promise(function (resolve, reject) {
        $.ajax({
            type: "POST",
            data: JSON.stringify({ idNotificacion: idNotificacion, idPersona, idPersona }),
            contentType: "application/json; charset=utf-8",
            url: "Notificaciones.aspx/EstadosFlujoNotificacion",
            success: function (data) {
                result = eval('(' + data.d + ')')
                if (result.Estado == 3) notSession();
                if (result.Estado == undefined || result.Estado == 1) {
                    flujo = "<div class='timelineH'>";
                    $.each(result.Table, function (i, val) {
                        flujo = flujo +
                            "<div class='timeLineItem'>" +
                            "<div class='timeline-badge " + (val.estadoExiste == 0 ? "" :"check") +"'>" +
                            "<i class='fas fa-check'></i>" +
                            "</div>" +
                            "<div class='leyenda'>" + val.ecngloest_cr + "</div>" +
                            "</div>" +
                            "<div class='lineTime'>" +
                            (i < result.Table.length-1 ? "<i class='fas fa-arrow-right'></i>" : "") +
                            "</div>"
                    });
                    resolve(flujo);
                }
                else {
                    resolve('');
                }
            },
            error: function (xhr, ajaxOptions, thrownError) {
                resolve('');
            },
        });
    });
}