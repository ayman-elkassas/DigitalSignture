$(function(){
    $("#ldap").click(function () {
        debugger
        var PdfPath = "";
        var numPage = 0;
        var SignImgPath = null;
        var certificateName = null;
        var docId = 0;

        debugger;
        $.ajax({
            type: "POST",
            url: "PdfSignNow.asmx/pdfSignNow",
            data: "{'PdfPath':'" + PdfPath + "','numPage':'" + numPage + "','SignImgPath':'" + SignImgPath + "','certificateName':'" + certificateName + "','docId':'" + docId + "'}",
            contentType: "application/json",
            datatype: "json",
            success: function (data) {
                alert(data.d);
            },
            error: function (error) {
                alert("Error In Sign");
            }
        });
    });
});