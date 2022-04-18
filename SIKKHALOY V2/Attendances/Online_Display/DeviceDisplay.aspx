<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DeviceDisplay.aspx.cs" Inherits="EDUCATION.COM.Attendances.Online_Display.DeviceDisplay" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <!-- Font Awesome -->
    <link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/font-awesome/4.7.0/css/font-awesome.min.css" />
    <!-- Bootstrap core CSS -->
    <link href="/CSS/bootstrap/bootstrap.css" rel="stylesheet" />
    <link href="mdb/css/mdb-core.css" rel="stylesheet" />
    <link href="https://cdnjs.cloudflare.com/ajax/libs/owl-carousel/1.3.3/owl.carousel.min.css" rel="stylesheet" />
    <link href="CSS/device-display.css" rel="stylesheet" />
</head>
<body>
    <form runat="server">
        <div class="student">
            <div class="summary z-depth-1">
                <ul class="list-group list-group-flush">
                    <li class="list-group-item active">Student: <span>500</span></li>
                    <li class="list-group-item">Current In: <span>100</span></li>
                    <li class="list-group-item">Total Out: <span>100</span></li>
                    <li class="list-group-item">Late: <span>100</span></li>
                    <li class="list-group-item">Absent: <span>100</span></li>
                    <li class="list-group-item">Late Absent: <span>100</span></li>
                </ul>
            </div>

             <div class="card card-body ml-3">
                <div class="slide-in str_wrap">
                    <div class="info-block">
                        <div class="card">
                            <div class="name-title">
                                <i class="fa fa-user-o" aria-hidden="true"></i>
                                Muhibullah
                            </div>
                            <img class="card-img-top" src="CSS/0068.jpg" />
                            <span class="notify-badge z-depth-2 Pre">Pre</span>
                            <div class="EntryDate">
                                <i class="fa fa-clock-o" aria-hidden="true"></i>
                                <span class="Etime">10:20am</span>
                            </div>
                        </div>
                    </div>

                    <div class="info-block">
                        <div class="card">
                            <div class="name-title">
                                <i class="fa fa-user-o" aria-hidden="true"></i>
                                Muhibullah
                            </div>
                            <img class="card-img-top" src="CSS/0068.jpg" />
                            <span class="notify-badge z-depth-2 Late">Late</span>
                            <div class="EntryDate">
                                <i class="fa fa-clock-o" aria-hidden="true"></i>
                                <span class="Etime">10:20am</span>
                            </div>
                        </div>
                    </div>

                    <div class="info-block">
                        <div class="card">
                            <div class="name-title">
                                <i class="fa fa-user-o" aria-hidden="true"></i>
                                Muhibullah
                            </div>
                            <img class="card-img-top" src="CSS/0068.jpg" />
                            <span class="notify-badge z-depth-2 Abs">Abs</span>
                            <div class="EntryDate">
                                <i class="fa fa-clock-o" aria-hidden="true"></i>
                                <span class="Etime">10:20am</span>
                            </div>
                        </div>
                    </div>
                </div>
        
                <div class="slide-out str_wrap">
                    <div class="info-block">
                        <div class="card">
                            <div class="name-title">
                                <i class="fa fa-user-o" aria-hidden="true"></i>
                                Muhibullah
                            </div>
                            <img class="card-img-top" src="CSS/0068.jpg" />
                            <span class="notify-badge z-depth-2 Pre">Pre</span>
                            <div class="EntryDate">
                                <i class="fa fa-clock-o" aria-hidden="true"></i>
                                <span class="Etime">10:20am</span>
                            </div>
                        </div>
                    </div>

                    <div class="info-block">
                        <div class="card">
                            <div class="name-title">
                                <i class="fa fa-user-o" aria-hidden="true"></i>
                                Muhibullah
                            </div>
                            <img class="card-img-top" src="CSS/0068.jpg" />
                            <span class="notify-badge z-depth-2 Late">Late</span>
                            <div class="EntryDate">
                                <i class="fa fa-clock-o" aria-hidden="true"></i>
                                <span class="Etime">10:20am</span>
                            </div>
                        </div>
                    </div>

                    <div class="info-block">
                        <div class="card">
                            <div class="name-title">
                                <i class="fa fa-user-o" aria-hidden="true"></i>
                                Muhibullah
                            </div>
                            <img class="card-img-top" src="CSS/0068.jpg" />
                            <span class="notify-badge z-depth-2 Abs">Abs</span>
                            <div class="EntryDate">
                                <i class="fa fa-clock-o" aria-hidden="true"></i>
                                <span class="Etime">10:20am</span>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <div class="student mt-2">
            <div class="summary z-depth-1">
                <ul class="list-group list-group-flush">
                    <li class="list-group-item active">Employee: <span>500</span></li>
                    <li class="list-group-item">Current In: <span>100</span></li>
                    <li class="list-group-item">Total Out: <span>100</span></li>
                    <li class="list-group-item">Late: <span>100</span></li>
                    <li class="list-group-item">Absent: <span>100</span></li>
                    <li class="list-group-item">Late Absent: <span>100</span></li>
                </ul>
            </div>

             <div class="card card-body ml-3">
                <div class="slide-in str_wrap">
                    <div class="info-block">
                        <div class="card">
                            <div class="name-title">
                                <i class="fa fa-user-o" aria-hidden="true"></i>
                                Muhibullah
                            </div>
                            <img class="card-img-top" src="CSS/0068.jpg" />
                            <span class="notify-badge z-depth-2 Pre">Pre</span>
                            <div class="EntryDate">
                                <i class="fa fa-clock-o" aria-hidden="true"></i>
                                <span class="Etime">10:20am</span>
                            </div>
                        </div>
                    </div>

                    <div class="info-block">
                        <div class="card">
                            <div class="name-title">
                                <i class="fa fa-user-o" aria-hidden="true"></i>
                                Muhibullah
                            </div>
                            <img class="card-img-top" src="CSS/0068.jpg" />
                            <span class="notify-badge z-depth-2 Late">Late</span>
                            <div class="EntryDate">
                                <i class="fa fa-clock-o" aria-hidden="true"></i>
                                <span class="Etime">10:20am</span>
                            </div>
                        </div>
                    </div>

                    <div class="info-block">
                        <div class="card">
                            <div class="name-title">
                                <i class="fa fa-user-o" aria-hidden="true"></i>
                                Muhibullah
                            </div>
                            <img class="card-img-top" src="CSS/0068.jpg" />
                            <span class="notify-badge z-depth-2 Abs">Abs</span>
                            <div class="EntryDate">
                                <i class="fa fa-clock-o" aria-hidden="true"></i>
                                <span class="Etime">10:20am</span>
                            </div>
                        </div>
                    </div>
                </div>
        
                <div class="slide-out str_wrap">
                    <div class="info-block">
                        <div class="card">
                            <div class="name-title">
                                <i class="fa fa-user-o" aria-hidden="true"></i>
                                Muhibullah
                            </div>
                            <img class="card-img-top" src="CSS/0068.jpg" />
                            <span class="notify-badge z-depth-2 Pre">Pre</span>
                            <div class="EntryDate">
                                <i class="fa fa-clock-o" aria-hidden="true"></i>
                                <span class="Etime">10:20am</span>
                            </div>
                        </div>
                    </div>

                    <div class="info-block">
                        <div class="card">
                            <div class="name-title">
                                <i class="fa fa-user-o" aria-hidden="true"></i>
                                Muhibullah
                            </div>
                            <img class="card-img-top" src="CSS/0068.jpg" />
                            <span class="notify-badge z-depth-2 Late">Late</span>
                            <div class="EntryDate">
                                <i class="fa fa-clock-o" aria-hidden="true"></i>
                                <span class="Etime">10:20am</span>
                            </div>
                        </div>
                    </div>

                    <div class="info-block">
                        <div class="card">
                            <div class="name-title">
                                <i class="fa fa-user-o" aria-hidden="true"></i>
                                Muhibullah
                            </div>
                            <img class="card-img-top" src="CSS/0068.jpg" />
                            <span class="notify-badge z-depth-2 Abs">Abs</span>
                            <div class="EntryDate">
                                <i class="fa fa-clock-o" aria-hidden="true"></i>
                                <span class="Etime">10:20am</span>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </form>

    <!-- JQuery -->
    <script type="text/javascript" src="https://cdnjs.cloudflare.com/ajax/libs/jquery/3.3.1/jquery.min.js"></script>
    <script src="js/jquery.liMarquee.js"></script>
    <script src="mdb/js/mdb-core.js"></script>
    <script>
        $(function () {
            $('.slide-in').liMarquee({
                direction: 'left',
                loop: -1,
                scrolldelay: 0,
                scrollamount: 50,
                circular: true,
                drag: true
            });

            $('.slide-out').liMarquee({
                direction: 'right',
                loop: -1,
                scrolldelay: 0,
                scrollamount: 50,
                circular: true,
                drag: true
            });
        });
    </script>
</body>
</html>
