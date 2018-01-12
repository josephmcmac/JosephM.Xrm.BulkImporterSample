
$(document).ready(function () {
    $("#import").hide();
    document.getElementById('upload').addEventListener('change', csvLoader.handleFileSelect, false);
    $(".importbutton").click(csvLoader.DoImport);
    $("#progressbar").progressbar({
        value: 0
    });
});


csvLoader = new Object();
csvLoader.DoImport = function () {

    function processRow(index) {
        var headerRow = csvLoader.CsvRows[0];
        var thisRow = csvLoader.CsvRows[index];
        if (thisRow == null || thisRow[0] == null || thisRow[0] == "") {
            $("#progressbar").progressbar("option", "value", 100);
            if (csvLoader.errorCount > 0)
            {
                $("#importerrordetails").html("Warning " + csvLoader.errorCount + " Errors Occured During The import");
            }
            else
            {
                $("#importerrordetails").html("");
            }
            return;
        }
        var rowObject = new Object();
        for (var i in headerRow) {
            rowObject[headerRow[i]] = thisRow[i];
        }
        var toJson = JSON.stringify(rowObject);
        var actionArgs = {
            RowJson: toJson
        };
        var processErrorFunc = function (response) {
            $('.csvtable tr:eq(' + index + ') td:eq(0)').html("<img class='progress' src='jmcg_bis_cross.jpg'  />");
            $('.csvtable tr:eq(' + index + ') td:last').html(response["message"]);

            $("#progressbar").progressbar("option", "value", index / ($('.csvtable tr').length - 1) * 100);
            csvLoader.errorCount = csvLoader.errorCount + 1;
            processRow(index + 1);
        };
        var processSuccessFunc = function (response) {
            $('.csvtable tr:eq(' + index + ') td:eq(0)').html("<img class='progress' src='jmcg_bis_tick.jpg' />");
            var url = "/main.aspx?pagetype=entityrecord&etn=" + response["RecordType"] + "&id=" + response["RecordId"];
            $('.csvtable tr:eq(' + index + ') td:eq(1)').html("<a target='_blank' href='" + url + "' >Open</a>");
            $('.csvtable tr:eq(' + index + ') td:last').html("");

            $("#progressbar").progressbar("option", "value", index / ($('.csvtable tr').length - 1) * 100);
            processRow(index + 1);
        };
        $('.csvtable tr:eq(' + index + ') td:eq(0)').html("<img class='progress' src='jmcg_bis_processing.gif' />");
        jmcg_bisWebApiUtility.executeAction("jmcg_bis_BulkImporterCreateRecord", actionArgs, processSuccessFunc, processErrorFunc);
    }
    csvLoader.errorCount = 0;
    processRow(1);
};

csvLoader.handleFileSelect = function (evt) {
    var files = evt.target.files; // FileList object

    // use the 1st file from the list
    f = files[0];

    var reader = new FileReader();

    // Closure to capture the file information.
    reader.onload = (function (theFile) {
        return function (e) {
            var array = csvLoader.CSVToArray(e.target.result);
            csvLoader.CsvRows = array;
            var html = "";
            $(".csvtablediv").html(html);
            html = html + "<table class='csvtable' >";
            var numberOfSystemColumns = 2;
            for (var i in array) {
                var row = array[i];
                if (i == 0) {
                    html = html + "<tr>"
                    for (var k = 0; k < numberOfSystemColumns; k++) {
                        html = html + "<th></th>"
                    }
                    for (var j in row) {
                        var thisColumn = row[j];
                        html = html + "<th>" + thisColumn + "</th>"
                    }
                    html = html + "<th></th>"
                    html = html + "</tr>"
                    $(".csvrows")
                }
                else {
                    html = html + "<tr>"
                    for (var k = 0; k < numberOfSystemColumns; k++) {
                        html = html + "<td class='system'></td>"
                    }
                    for (var j in row) {
                        var thisColumn = row[j];
                        html = html + "<td>" + thisColumn + "</td>"
                    }
                    html = html + "<td></td>"
                    html = html + "</tr>"
                }
            }
            html = html + "</table>";
            $(".csvtablediv").html(html);
            $("#import").show();
            $("#fileselector").hide();
        };
    })(f);

    // Read in the image file as a data URL.
    reader.readAsText(f);
};

//copied https://www.bennadel.com/blog/1504-ask-ben-parsing-csv-strings-with-javascript-exec-regular-expression-command.htm
csvLoader.CSVToArray = function (strData, strDelimiter) {
    // Check to see if the delimiter is defined. If not,
    // then default to comma.
    strDelimiter = (strDelimiter || ",");

    // Create a regular expression to parse the CSV values.
    var objPattern = new RegExp(
        (
            // Delimiters.
            "(\\" + strDelimiter + "|\\r?\\n|\\r|^)" +

            // Quoted fields.
            "(?:\"([^\"]*(?:\"\"[^\"]*)*)\"|" +

            // Standard fields.
            "([^\"\\" + strDelimiter + "\\r\\n]*))"
        ),
        "gi"
    );


    // Create an array to hold our data. Give the array
    // a default empty first row.
    var arrData = [[]];

    // Create an array to hold our individual pattern
    // matching groups.
    var arrMatches = null;


    // Keep looping over the regular expression matches
    // until we can no longer find a match.
    while (arrMatches = objPattern.exec(strData)) {

        // Get the delimiter that was found.
        var strMatchedDelimiter = arrMatches[1];

        // Check to see if the given delimiter has a length
        // (is not the start of string) and if it matches
        // field delimiter. If id does not, then we know
        // that this delimiter is a row delimiter.
        if (
            strMatchedDelimiter.length &&
            strMatchedDelimiter !== strDelimiter
        ) {

            // Since we have reached a new row of data,
            // add an empty row to our data array.
            arrData.push([]);

        }

        var strMatchedValue;

        // Now that we have our delimiter out of the way,
        // let's check to see which kind of value we
        // captured (quoted or unquoted).
        if (arrMatches[2]) {

            // We found a quoted value. When we capture
            // this value, unescape any double quotes.
            strMatchedValue = arrMatches[2].replace(
                new RegExp("\"\"", "g"),
                "\""
            );

        } else {

            // We found a non-quoted value.
            strMatchedValue = arrMatches[3];

        }


        // Now that we have our value string, let's add
        // it to the data array.
        arrData[arrData.length - 1].push(strMatchedValue);
    }

    // Return the parsed data.
    return (arrData);
}