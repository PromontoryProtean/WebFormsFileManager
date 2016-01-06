$(document).ready(function () {
    $("#file-view-mode-btn-group input:radio").change(function () {
        var selectedValue = $("#file-view-mode-btn-group input:radio:checked").val();
        if (selectedValue == "Details")
            Cookies.set("FileManagerViewMode", "Details");
        else
            Cookies.set("FileManagerViewMode", "Tiles");
        window.location.href = window.location.href;
    });

    $("#start-directory-btn-group input:radio").change(function () { window.location = $("#start-directory-btn-group input:radio:checked").val(); });

    $(".thumb").click(function () {
        //var filePath = $(this).attr("src").split("?")[0];
        var filePath = $(this).data("path");
        var uri = "/api/MediaFile/GetMediaFile?path=" + encodeURIComponent(filePath);
        //console.log("Media File Request for: " + uri);
        var container = $(".tile-details");
        container.empty();
        container.append("<h2>File Details</h2>");

        $.getJSON(uri).done(function (data) {
            var s = "<p>File Name: " + data.FileName
                + "<br />File Path: " + data.FilePath
                + "<br />Date/Time Created: " + formatDate(data.CreatedUtc)
                + "<br />Date/Time Updated: " + formatDate(data.LastWriteUtc)
                + "<br />File Size: " + data.SizeKb + " KB";
            if (data.IsImage) {
                {
                    s += "<br />Dimensions: " + data.Dimensions + "</p>";
                    if (filePath.indexOf("/secure-files") == 0)
                        s += "<img src=\"/secure-image.ascx/" + data.FileName + "?width=350&height=500&mode=max&dir=" + encodeURIComponent(filePath.replace("/" + data.FileName, "")) + "\" alt=\"" + data.FileName + "\" />";
                    else
                        s += "<img src=\"" + filePath + "?width=350&height=500&mode=max\" alt=\"" + data.FileName + "\" />";
                }
            }
            else { s += "</p>" };
            container.append(s);
        }).fail(function () {
            container.append("<p>There was a problem loading the file. Please try a different file.</p>");
        });
    });

    $("#uploader").pluploadQueue({
        runtimes: "html5",
        url: "/upload.ashx?dir=" + currentDir,
        max_file_size: "32mb",
        chunk_size: "1mb",
        unique_names: false,
        preinit: {
            UploadFile: function (up, file) {
                var doResizeImages = $("#DoResizeImages").is(":checked");
                var width = $("#WidthField").val().trim();
                if (width.length == 0)
                    width = 0;
                var height = $("#HeightField").val().trim();
                if (height.length == 0)
                    height = 0;

                var imageSettings = { doResizeImages: doResizeImages, width: width, height: height };
                Cookies.set("ImageUploadSettings", $.param(imageSettings));

                if ($("#DoResizeImages").is(":checked")) {
                    var qs = "";
                    if (width > 0)
                        qs += "&width=" + width;
                    if (height > 0)
                        qs += "&height=" + height;
                    if (qs.length > 0)
                        up.setOption("url", "/upload.ashx?dir=" + currentDir + qs);
                }
            }
        },
        init: {
            UploadComplete: function (up, files) {
                // Called when all files are either uploaded or failed
                if (up.total.failed == 0) {
                    window.location.href = window.location.href;
                }
            },
            Error: function (up, args) {
                alert("An error has occured: " + error.message);
            }
        }
    });
});

function ToggleCheckboxes(sender) {
    $("input[type='checkbox'].DetailsViewItemCheckbox").each(function () {
        this.checked = sender.checked;
    });
}

function formatDate(date) {
    date = new Date(date);
    var M = date.getMonth() + 1;
    var d = date.getDate();
    var yyyy = date.getFullYear();
    var h = date.getHours();
    var tt = (h > 12) ? "PM" : "AM";
    h = (h > 12) ? h - 12 : h;
    var m = date.getMinutes();
    m = (m < 10) ? '0' + m : m;
    return M + "/" + d + "/" + yyyy + " " + h + ":" + m + " " + tt;
}