<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Notificaciones.aspx.cs" Inherits="RIF_Front.Notificaciones" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <link rel="stylesheet" href="./css/bootstrap.css" />
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div class="main-panel">
            <div class="content">
                <div class="page-inner">
                    <div class="page-header">
                        <h4 class="page-title">Notificaciones</h4>
                        <ul class="breadcrumbs">
                            <li class="nav-home">
                                <a href="javaScript:Home()">
                                    <i class="flaticon-home"></i>
                                </a>
                            </li>
                        </ul>
                    </div>


                    <div class="row">
                        <div class="col-sm-12 col-md-12 col-xl-10 col-lg-10">
                            <div class="row">
                                <div class="col-sm-12 col-md-12 col-xl-4 col-lg-4 cursor" onclick="GrillaNotificacion('SOFI')">
                                    <div class="card card-stats card-primary card-round">
                                        <div class="card-body">
                                            <div class="row">
                                                <div class="col-2">
                                                    <div class="mg-20 icon-big text-center">
                                                        <i class="flaticon-pen"></i>
                                                    </div>
                                                </div>
                                                <div class="col-10 col-stats">
                                                    <div class="numbers">
                                                        <p class="card-category">Solicitudes de Firma</p>
                                                        <h4 class="card-title"><span id="SOFIPEN" runat="server"></span>&nbsp;de&nbsp;<span id="SOFITOT" runat="server"></span>&nbsp;Pendientes</h4>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                                <div class="col-sm-12 col-md-12 col-xl-4 col-lg-4 cursor" onclick="GrillaNotificacion('SODO')">
                                    <div class="card card-stats card-primary card-round">
                                        <div class="card-body">
                                            <div class="row">
                                                <div class="col-2">
                                                    <div class="mg-20 icon-big text-center">
                                                        <i class="flaticon-file"></i>
                                                    </div>
                                                </div>
                                                <div class="col-10 col-stats">
                                                    <div class="numbers">
                                                        <p class="card-category">Solicitudes de Documentos</p>
                                                        <h4 class="card-title"><span id="SODOPEN" runat="server"></span>&nbsp;de&nbsp;<span id="SODOTOT" runat="server"></span>&nbsp;Pendientes</h4>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                                <div class="col-sm-12 col-md-12 col-xl-4 col-lg-4 cursor" onclick="GrillaNotificacion('MEIN')">
                                    <div class="card card-stats card-primary card-round">
                                        <div class="card-body ">
                                            <div class="row">
                                                <div class="col-2">
                                                    <div class="mg-20 icon-big text-center">
                                                        <i class="flaticon-chat-6"></i>
                                                    </div>
                                                </div>
                                                <div class="col-10 col-stats">
                                                    <div class="numbers">
                                                        <p class="card-category">Mensajes Informativos</p>
                                                        <h4 class="card-title"><span id="MEINTOT" runat="server"></span>&nbsp;Enviados</h4>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="row">
                    <div class="col-lg-12">
                        <div class="card full-height">
                            <div class="card-header no-margin-vert">
                                <div class="card-head-row">
                                    <div class="GroupSearchFilter">
                                        <div class="row">
                                            <div class="col-sm-12 col-xs-12 col-md-8 col-lg-8 col-xl-8">
                                                <div class="group-input">
                                                    <input type="search" class="dt-input-search" id="filterSearch" placeholder="" />
                                                    <i class="icon-magnifier"></i>
                                                </div>
                                            </div>
                                            <div class="col-sm-12 col-xs-12 col-md-4 col-lg-4 col-xl-4">
                                                <div class="dt-button filter">
                                                    <i class="fas fa-filter"></i>
                                                    Filtros
                                                </div>
                                            </div>
                                        </div>
                                        <div class="panelFilters" style="display: none">
                                            <div>
                                                <div class="closeFilter">
                                                    <div class="x dt-button_x">X</div>
                                                </div>
                                                <div class="row">
                                                    <div class="col-lg-12 col-xl-12">
                                                        <div>
                                                            <h6 class="text-uppercase fw-bold mb-1">Fecha de Notificación</h6>
                                                            <small>Rango de Fechas</small>
                                                        </div>
                                                        <input type="text" name="daterange" value="" class="inputFilter" />
                                                        <div class="form-group compact mt-2">

                                                            <label class="switchCheck floatCheck">
                                                                <input id="chkMesActual" type="checkbox" class="form-group" />
                                                                <span class="slider round"></span>
                                                            </label>
                                                            <div class="margin-check-aling">Mes Actual</div>
                                                        </div>
                                                    </div>
                                                    <div class="separator-filter"></div>
                                                    <div class="col-lg-12 col-xl-12">
                                                        <div>
                                                            <h6 class="text-uppercase fw-bold mb-1">Estado</h6>
                                                            <small>Estado actual de la notificación</small>
                                                        </div>
                                                        <select name="states[]" multiple="true" class="form-control selectFilter" id="cmbEstadoNotificacion" runat="server" datatextfield="DerDesReg_ds" datavaluefield="DerDomReg_cr"></select>
                                                    </div>
                                                    <div class="separator-filter"></div>
                                                    <div class="col-lg-12 col-xl-12">
                                                        <div>
                                                            <h6 class="text-uppercase fw-bold mb-1">Tipo</h6>
                                                            <small>Tipo de notificación</small>
                                                        </div>
                                                        <select name="states[]" multiple="true" class="form-control selectFilter" id="cmbTipoNotificacionFilter" runat="server" datatextfield="cnonomcno_ds" datavaluefield="cnocodcno_id"></select>
                                                    </div>
                                                </div>
                                            </div>
                                            <div class="mt-5">
                                                <div class="row">
                                                    <div class="col-sm-12 col-xs-12 col-md-12 col-lg-4 col-xl-4">
                                                        <div class="btn btn-outline-primary w-100 mb-2">
                                                            Limpiar
                                                        </div>
                                                    </div>
                                                    <div class="col-sm-12 col-xs-12 col-md-12 col-lg-4 col-xl-4">
                                                        <div class="btn btn-primary w-100 mb-2">
                                                            Cancelar
                                                        </div>
                                                    </div>
                                                    <div class="col-sm-12 col-xs-12 col-md-12 col-lg-4 col-xl-4">
                                                        <div class="btn btn-primary w-100 mb-2">
                                                            Aplicar
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="card-tools">
                                        <div class="dt-button-only-icon">
                                            <i class="icon-options-vertical"></i>
                                        </div>
                                        <ul class="nav nav-pills nav-secondary nav-pills-no-bd nav-sm" id="pills-tab" role="tablist" style="display: none">
                                            <li class="nav-item">
                                                <a class="nav-link" id="pills-today" data-toggle="pill" href="#pills-today" role="tab" aria-selected="false">Antiguas</a>
                                            </li>
                                            <li class="nav-item">
                                                <a class="nav-link" id="pills-week" data-toggle="pill" href="#pills-week" role="tab" aria-selected="false">Este Mes</a>
                                            </li>
                                        </ul>
                                    </div>
                                </div>
                            </div>
                            <div class="card-body gridNotificaciones">
                                <table runat="server" id="tblNotificaciones1" style="width: 100%"></table>
                            </div>
                        </div>
                    </div>
                    <div class="col-lg-12" style="display: none">
                        <div class="row" runat="server" id="CalugasResumen">
                        </div>
                    </div>
                </div>
               
                <div class="card" style="display: none">
                    <div class="card-header">
                        <div id="newNotificacion" class="btn btn-primary btn-round float-right btn-space" data-toggle="modal" data-target="#ModalNotificacion" onclick="LimpiaNotificacion()">
                            <i class="fas fa-plus"></i>
                            Nueva Notificación
                        </div>
                    </div>
                    <div class="card-body">
                        <div class="row">
                            <div class="col-xl-12 col-lg-12 col-md-12">
                                <div style="min-height: 348px" class="table-responsive">
                                    <table runat="server" id="tblNotificaciones" class="table-sm display table"></table>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

    </form>
</body>
</html>

