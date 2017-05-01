$(document).ready(function () {

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
    // Get a reference to our hub
    $.connection.hub.url = "signalr";
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
        span3.setAttribute("group", clientOutput.Group);
        li3.className = "node";
        li3.appendChild(span3);
        ul3.appendChild(li3);

        if (clientOutput.Group === null || clientOutput.Group === "") {
            if (document.getElementById(clientOutput.ID) === null) {
                document.getElementById("noCategory").appendChild(ul3);
            }
        }
        else if (document.getElementById(clientOutput.Group) === null) {
            var li2 = document.createElement('li');
            var span2 = document.createElement('span');
            span2.className = "leaf";
            var span2_ = document.createElement('span');
            span2_.className = "node-toggle";
            span2.textContent = clientOutput.Group;
            li2.appendChild(span2);
            li2.appendChild(span2_);
            li2.id = clientOutput.Group;
            li2.className = "node";
            document.getElementById("rootNode").appendChild(li2);
            if (document.getElementById(clientOutput.ID) === null) {
                document.getElementById(clientOutput.Group).appendChild(ul3);
            }
        } else if (document.getElementById(clientOutput.ID) === null) {
            document.getElementById(clientOutput.Group).appendChild(ul3);
        }
    };

    hub.client.pluginsMessage = function (clientOutput) {
        console.log("pluginsMessage");

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
                var position = JSON.parse(localStorage.getItem(clientOutput.ID + '_' + plugin.PluginUID));
                var plugDiv = document.createElement('div');
                var frameDiv = document.createElement('div');
                table = document.createElement('table');
                var title = document.createElement('h2');
                title.innerHTML = plugin.PluginName;
                frameDiv.style.padding = "10px";
                plugDiv.classList.add("ui-widget-content");
                plugDiv.classList.add("draggable");
                plugDiv.id = plugin.PluginUID;
                $(plugDiv).css({ position: "absolute" });
                $(plugDiv).css("border-width", "1px");
                $(plugDiv).css({ top: position.Top + "px"});
                $(plugDiv).css({ left: position.Left + "px"});

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

                var separator = document.createElement('hr');
                separator.style.marginTop = "0px";
                separator.style.marginBottom = "0px";
                separator.style.height = "1px";

                frameDiv.appendChild(title);
                frameDiv.appendChild(separator);
                frameDiv.appendChild(table);
                plugDiv.appendChild(frameDiv);

                $("#containment-wrapper").append(plugDiv);
            }

        });
        refreshDraggable();
    };

    hub.client.previewCritical = function (criticalValues) {
        console.log("previewCritical");
        $("#containment-wrapper").empty();

        var newResultTable;
        newResultTable = document.createElement("table");

        //make table header
        var headerRow = document.createElement("tr");
        var groupHead = document.createElement("th");
        groupHead.textContent = "Group";
        headerRow.appendChild(groupHead);
        var stationHead = document.createElement("th");
        stationHead.textContent = "Station";
        headerRow.appendChild(stationHead);
        var pluginHead = document.createElement("th");
        pluginHead.textContent = "Plugin";
        headerRow.appendChild(pluginHead);
        var valueHead = document.createElement("th");
        valueHead.textContent = "Value";
        headerRow.appendChild(valueHead);
        newResultTable.appendChild(headerRow);


        criticalValues.forEach(function (clientOutput) {

            clientOutput.CollectionList.forEach(function (plugin) {

                plugin.PluginOutputList.forEach(function (pluginElement) {

                    pluginElement.Values.forEach(function (simplePluginElement) {

                        var row = document.createElement("tr");
                        var group = document.createElement("td");
                        group.textContent = clientOutput.Group;
                        row.appendChild(group);
                        var station = document.createElement("td");
                        station.textContent = clientOutput.PCName;
                        row.appendChild(station);
                        var pluginRow = document.createElement("td");
                        pluginRow.textContent = plugin.PluginName;
                        row.appendChild(pluginRow);
                        var value = document.createElement("td");
                        value.textContent = pluginElement.PropertyName + " - " + simplePluginElement.Value;
                        if (simplePluginElement.IsCritical) {
                            value.className = "alertRow";
                        }
                        //TODO warning
                        /*if (simplePluginElement.IsWarning) {
                            cellValue.className = "warningRow";
                        }*/
                        row.appendChild(value);
                        newResultTable.appendChild(row);
                    });
                });
            });
        });

        $("#containment-wrapper").append(newResultTable);
    };

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
    };

    hub.client.UpdateUsersOnlineCount = function (count) {
        $('#usersCount').text(count);
    };

    hub.client.SavePositionToCookies = function (positions) {
        console.log(positions);
        positions.forEach(function (plugSettings) {

            var pluginPositionID = plugSettings.ComputerID + '_' + plugSettings.PluginUID;
            console.log(pluginPositionID);
            localStorage.setItem(pluginPositionID, JSON.stringify(plugSettings.HTMLPosition));
        });
    };

    // Start the connection
    $.connection.hub.start().done(function () {
        vm.connected(true);
    });

    $('.toggle').toggles({ drag: false });
    $('.toggle').on('toggle', refreshDraggable).on('toggle', savePositonOnToggleOff);
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


// call view with warnings only
function onRootNodeClick() {
    $("#mainTitle").html("Warnings");
    if ($('.toggle-on').hasClass('active')){
        $('.toggle').toggles({ drag: false });
    }
    $("#editableSwitch").hide();

    $.connection.hub.url = "signalr";
    var hub = $.connection.MyHub;
    hub.server.callWarningsView();
}

// on node click change title to actual group / machine (station)
function onNodeClick(object) {
    $("#mainTitle").html(object.getAttribute('group') + "/" + object.textContent);
    $("#containment-wrapper").empty();
    $("#editableSwitch").show();
    $.connection.hub.url = "signalr";
    var hub = $.connection.MyHub;
    hub.server.nodeClick(object.id, object.textContent, object.getAttribute('group'));
}

function onSwitchClick() {
    $.connection.hub.url = "signalr";
    var hub = $.connection.MyHub;
    hub.server.onSwitchClick();
}

function onLoadClick() {
    $.connection.hub.url = "signalr";
    var hub = $.connection.MyHub;
    hub.server.onLoadClick();
}

function refreshDraggable() {
    console.log("editDraggable()");
    if ($('.toggle-on').hasClass('active')) {
        $('.draggable').each(function () {
            $(this).draggable({ disabled: false });
            $(this).draggable({ containment: "#containment-wrapper", snap: true });
        });
    }
    else {
        $('.draggable').each(function () {
            $(this).draggable({ disabled: true });
        });
    }
}

function savePositonOnToggleOff() {

    if ($('.toggle-off').hasClass('active')) {
        $.connection.hub.url = "signalr";
        var hub = $.connection.MyHub;
        $('.draggable').each(function () {
            var top = $(this).position().top;
            var left = $(this).position().left;
            var computerID = $('.node.active').find('span')[0].id;                
            var pluginID = $(this)[0].id;
            console.log($(this)[0].id);
            hub.server.saveHTMLPostion(computerID, pluginID, top, left);
        })
    }
}