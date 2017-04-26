$(function () {

    // The view model that is bound to our view
    var ViewModel = function () {
        var self = this;

        // Whether we're connected or not
        self.connected = ko.observable(false);

        // Collection of machines that are connected
        self.propValues = ko.observableArray();
    };

    // Instantiate the viewmodel..
    var vm = new ViewModel();

    // .. and bind it to the view
    ko.applyBindings(vm, $("#computerInfo")[0]);

    console.log("before start connection");
    //console.log($.connection);
    // Get a reference to our hub
    $.connection.hub.url = "signalr";
    //$.connection.hub.url = "http://localhost:8000/signalr";
    var hub = $.connection.MyHub

    hub.client.activateTree = function (clientOutput) {
        //console.log("activateTree");

        var treeview = document.getElementById("treeview");
        if (treeview === null) {
            var treeDiv = document.getElementById("treeDiv");
            var treeView = document.createElement("div");
            var ul = document.createElement("ul");
            var ul2 = document.createElement("ul");
            var li = document.createElement("li");
            var spanTmp1 = document.createElement('span');
            var spanTmp2 = document.createElement('span');
            var img = document.createElement('img');

            treeView.className = "treeview";
            treeView.setAttribute("data-role", "treeview");
            treeView.id = "treeview";
            li.className = "node active";
            spanTmp1.className = "leaf";
            img.className = "icon";
            spanTmp1.setAttribute("onclick", "onRootNodeClick()");
            spanTmp1.appendChild(img);
            spanTmp1.textContent = "Machines";
            spanTmp2.className = "node-toggle";

            li.appendChild(spanTmp1);
            li.appendChild(spanTmp2);
            ul2.id = "rootNode";
            li.appendChild(ul2);
            ul.appendChild(li);
            treeView.appendChild(ul);
            treeDiv.appendChild(treeView);
        }


        var ul3 = document.createElement('ul');
        var li3 = document.createElement('li');
        var span3 = document.createElement('span');
        span3.className = "leaf";
        span3.setAttribute("onclick", "onNodeClick(this)");
        span3.textContent = clientOutput.PCName;
        span3.id = clientOutput.ID;
        span3.setAttribute("customer", clientOutput.Customer);
        li3.className = "node";
        li3.appendChild(span3);
        ul3.appendChild(li3);

        if (clientOutput.Customer === null || clientOutput.Customer === "") {
            if (document.getElementById(clientOutput.ID) === null) {
                document.getElementById("noCategory").appendChild(ul3);
            }
        }
        else if (document.getElementById(clientOutput.Customer) === null) {
            var li2 = document.createElement('li');
            var span2 = document.createElement('span');
            span2.className = "leaf";
            var span2_ = document.createElement('span');
            span2_.className = "node-toggle";
            span2.textContent = clientOutput.Customer;
            li2.appendChild(span2);
            li2.appendChild(span2_);
            li2.id = clientOutput.Customer;
            li2.className = "node";
            document.getElementById("rootNode").appendChild(li2);
            if (document.getElementById(clientOutput.ID) === null) {
                document.getElementById(clientOutput.Customer).appendChild(ul3);
            }
        } else if (document.getElementById(clientOutput.ID) === null) {
            document.getElementById(clientOutput.Customer).appendChild(ul3);
        }
    }

    hub.client.deactivateTree = function () {
        //$(document).find('#treeview').remove();
        //var tree = document.getElementById("treeDiv");
    }

    hub.client.pluginsMessage = function (clientOutput) {
        console.log("pluginsMessage");

        //var newResultTable = document.createElement('table');
        //var pcName = document.createElement('tr');
        //var nameCell = document.createElement('th');
        //var lastUpdate = document.createElement('td');
        //lastUpdate.textContent = "Last Update: " + clientOutput.LastUpdate;
        //nameCell.textContent = clientOutput.PCName;
        //pcName.appendChild(nameCell);
        //pcName.appendChild(lastUpdate);
        //newResultTable.appendChild(pcName);
        //newResultTable.id = "resultTable";

        clientOutput.CollectionList.forEach(function (plugin) {

            if ($("#" + plugin.PluginUID).length) {
                //$("#" + plugin.PluginUID).replaceWith(plugDiv);

                var table = document.createElement('table');

                plugin.PluginOutputList.forEach(function (pluginElement) {
                    var row = document.createElement('tr');
                    var cellName = document.createElement('td');
                    cellName.textContent = pluginElement.PropertyName;
                    row.appendChild(cellName);

                    pluginElement.Values.forEach(function (simplePluginElement) {
                        var cellValue = document.createElement('td');
                        cellValue.textContent = simplePluginElement.Value;

                        if (simplePluginElement.IsCritical) {
                            cellValue.className = "alertRow";
                        }
                        row.appendChild(cellValue);
                    });
                    table.appendChild(row);
                });

                $("#" + plugin.PluginUID).find('table').replaceWith(table);
            }
            else {

                var plugDiv = document.createElement('div');
                var frameDiv = document.createElement('div');
                table = document.createElement('table');
                var title = document.createElement('h2');
                title.innerHTML = plugin.PluginName;
                frameDiv.style.padding = "10px";
                plugDiv.classList.add("ui-widget-content");
                plugDiv.classList.add("draggable");
                plugDiv.id = plugin.PluginUID;
                //$(plugDiv).draggable({ containment: "#containment-wrapper", snap: true });
                $(plugDiv).css({ position: "absolute" });
                //$(plugDiv).css({ left: "100px" });
                //$(plugDiv).css({ top: "100px" });
                $(plugDiv).css("border-width", "0px");

                plugin.PluginOutputList.forEach(function (pluginElement) {
                    var row = document.createElement('tr');
                    var cellName = document.createElement('td');
                    cellName.textContent = pluginElement.PropertyName;
                    row.appendChild(cellName);

                    pluginElement.Values.forEach(function (simplePluginElement) {
                        var cellValue = document.createElement('td');
                        cellValue.textContent = simplePluginElement.Value;

                        if (simplePluginElement.IsCritical) {
                            cellValue.className = "alertRow";
                        }
                        row.appendChild(cellValue);
                    });
                    table.appendChild(row);
                });

                frameDiv.appendChild(title);
                frameDiv.appendChild(table);
                plugDiv.appendChild(frameDiv);

                $("#containment-wrapper").append(plugDiv);
            }

        });
    }

    hub.client.previewCritical = function (criticalValues) {
        console.log("previewCritical");

        //criticalValues.forEach(function (clientOutput) {
        //    var newResultTable;
        //    if (document.getElementById(clientOutput.Customer + "DIV") === null) {
        //        var newResultDiv = document.createElement("div");
        //        newResultDiv.id = clientOutput.Customer + "DIV";
        //        var headerText = document.createElement("h1");
        //        headerText.textContent = clientOutput.Customer;
        //        newResultTable = document.createElement("table");
        //        newResultTable.id = clientOutput.Customer + "TABLE";
        //        newResultDiv.appendChild(headerText);
        //        newResultDiv.appendChild(newResultTable);
        //        document.getElementById("tableDiv").appendChild(newResultDiv);
        //    }
            
        //    newResultTable = document.getElementById(clientOutput.Customer + "TABLE");
        //    //console.log(newResultTable);

        //    var pcName = document.createElement("tr");
        //    var nameCell = document.createElement("th");
        //    var lastUpdate = document.createElement("td");
        //    nameCell.textContent = clientOutput.PCName;
        //    pcName.appendChild(nameCell);
        //    newResultTable.appendChild(pcName);

        //    clientOutput.CollectionList.forEach(function (plugin) {
        //        var headRow = document.createElement("tr");
        //        var headCell = document.createElement("th");
        //        headCell.textContent = plugin.PluginName;
        //        headCell.setAttribute("colspan", "100");
        //        headRow.appendChild(headCell);
        //        newResultTable.appendChild(headRow);
           
        //        plugin.PluginOutputList.forEach(function (pluginElement) {
        //            var row = document.createElement("tr");
        //            var cellName = document.createElement("td");
        //            cellName.textContent = pluginElement.PropertyName;
        //            row.appendChild(cellName);

        //            pluginElement.Values.forEach(function (simplePluginElement) {
        //                var cellValue = document.createElement("td");
        //                cellValue.textContent = simplePluginElement.Value;

        //                if (simplePluginElement.IsCritical) {
        //                    cellValue.className = "alertRow";
        //                }
        //                row.appendChild(cellValue);
        //            });
        //            newResultTable.appendChild(row);
        //        });
        //    });
        //});
    }

    hub.client.InitMainDiv = function () {
        //console.log("initMainDiv");

        newMainDiv = document.createElement("div");
        newMainDiv.className = "grid";
        newMainDiv.id = "mainDiv";
        var rowCells4 = document.createElement('div');
        rowCells4.className = "row cells4";
        var cell = document.createElement('div');
        cell.classList.add("cell");
        cell.classList.add("ui-widget-header");
        cell.id = "treeDiv";
        //var cellcollspan3 = document.createElement('div');
        //cellcollspan3.className = "cell collspan3";
        //cellcollspan3.id = "tableDiv";
        rowCells4.appendChild(cell);
        //rowCells4.appendChild(cellcollspan3);
        newMainDiv.appendChild(rowCells4);

        var mainDiv = document.getElementById("mainDiv");
        if (mainDiv === null) {
            document.body.appendChild(newMainDiv);
        }
        else {
            document.body.replaceChild(newMainDiv, mainDiv);
        }
    }

    hub.client.UpdateUsersOnlineCount = function (count) {
        $('#usersCount').text(count);
    }

    // Start the connection
    $.connection.hub.start().done(function () {
        vm.connected(true);
    });
});

function checkFirstVisit() {

    if (document.cookie.indexOf('checkRefresh') === -1) {
        // cookie doesn't exist, create it now
        document.cookie = 'checkRefresh=1';
    }
    else {
        // not first visit, so alert
        //alert('You refreshed!');
        //console.log('You refreshed!');

        /*$.connection.hub.url = "http://localhost:15123/signalr";
        var hub = $.connection.MyHub;
        hub.server.onRefresh();
        */

        // Start the connection
        /*$.connection.hub.start().done(function () {
            //vm.connected(true);
        });*/
    }
}

function onRootNodeClick() {
    $.connection.hub.url = "signalr";
    var hub = $.connection.MyHub;
    hub.server.onSwitchClick();
}

function onNodeClick(object) {

    //console.log("onNodeClick");
    //console.log(object);   

    $("#stationTitle").html(object.getAttribute('customer') + "/" + object.textContent);
    $.connection.hub.url = "signalr";
    //$.connection.hub.url = "http://localhost:8000/signalr";
    var hub = $.connection.MyHub;

    //console.log(object.getAttribute('customer'));
    hub.server.nodeClick(object.id, object.textContent, object.getAttribute('customer'));
}

function onSwitchClick() {
    //console.log("onSwitchClick");

    $.connection.hub.url = "signalr";
    //$.connection.hub.url = "http://localhost:8000/signalr";
    var hub = $.connection.MyHub;

    hub.server.onSwitchClick();
}

function onLoadClick() {
    //console.log("onLoadClick");
    $.connection.hub.url = "signalr";
    //$.connection.hub.url = "http://localhost:8000/signalr";
    var hub = $.connection.MyHub;

    hub.server.onLoadClick();
}

$(document).ready(function () {
    $('.toggle').on('toggle', function () {
        console.log("onToggle()");
        if ($('.toggle-on').hasClass('active')) {
            console.log("toggle-on active");
            $('.draggable').each(function () {
                $(this).draggable({ disabled: false });
                $(this).draggable({ containment: "#containment-wrapper", snap: true });
            });
        }
        else {
            console.log("toggle-off active");
            $('.draggable').each(function () {
                $(this).draggable({ disabled: true });
            });
        }
    });
});