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

    // Get a reference to our hub
    $.connection.hub.url = "http://localhost:15123/signalr";
    var hub = $.connection.MyHub
  

    hub.client.activateTree = function (clientOutput) {
        console.log("activateTree");

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

    hub.client.pluginsMessage = function (clientOutput) {
        console.log("pluginsMessage");

        var result = document.createElement('table');
        var pcName = document.createElement('tr');
        var nameCell = document.createElement('th');
        var lastUpdate = document.createElement('td');
        lastUpdate.textContent = "Last Update: " + clientOutput.LastUpdate;
        nameCell.textContent = clientOutput.PCName;
        pcName.appendChild(nameCell);
        pcName.appendChild(lastUpdate);
        result.appendChild(pcName);

        clientOutput.CollectionList.forEach(function (plugin) {
            var headRow = document.createElement('tr');
            var headCell = document.createElement('th');
            headCell.textContent = plugin.PluginName;
            headCell.setAttribute("colspan", "100");
            headRow.appendChild(headCell);
            result.appendChild(headRow);
           
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
                result.appendChild(row);
            });
        });
        result.id = "resultTable";

        console.log("resultTable: ");
        console.log(result);
        var oldTable = document.getElementById("resultTable");
        var parent;
        var noResult;
        var activeNode = document.body.getElementsByClassName("active")[0];

        if (activeNode !== null) {
            var activeNodeID = activeNode.firstChild.id;

            if (activeNodeID === clientOutput.ID) {
                oldTable = document.getElementById("resultTable");
                parent = oldTable.parentElement;
                parent.replaceChild(result, oldTable);
            }
            else {
                noResult = document.createElement('p');
                noResult.textContent = "No result for selected machine!";
                noResult.id = "resultTable";
                oldTable = document.getElementById("resultTable");
                parent = oldTable.parentElement;
                parent.replaceChild(noResult, oldTable);
            }
        }
        else {
            noResult = document.createElement('p');
            noResult.textContent = "No result for selected machine!";
            noResult.id = "resultTable";
            oldTable = document.getElementById("resultTable");
            parent = oldTable.parentElement;
            parent.replaceChild(noResult, oldTable);
        }
    }

    hub.client.previewCritical = function (criticalValues) {
        console.log("previewCritical");

        var result = document.createElement('table');

        criticalValues.forEach(function (clientOutput) {

            var pcName = document.createElement('tr');
            var nameCell = document.createElement('th');
            var lastUpdate = document.createElement('td');
            nameCell.textContent = clientOutput.PCName;
            pcName.appendChild(nameCell);
            result.appendChild(pcName);

            clientOutput.CollectionList.forEach(function (plugin) {
                var headRow = document.createElement('tr');
                var headCell = document.createElement('th');
                headCell.textContent = plugin.PluginName;
                headCell.setAttribute("colspan", "100");
                headRow.appendChild(headCell);
                result.appendChild(headRow);
           
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
                    result.appendChild(row);
                });
            });
            result.id = "resultTable";

            var oldTable = document.getElementById("resultTable");
            var parent;

            oldTable = document.getElementById("resultTable");
            parent = oldTable.parentElement;
            parent.replaceChild(result, oldTable);
        });

    }

    // Start the connection
    $.connection.hub.start().done(function () {
        vm.connected(true);
    });
});

function checkFirstVisit() {
    /*$.connection.hub.url = "http://localhost:15123/signalr";
    var hub = $.connection.MyHub;*/


    // Start the connection
    /*$.connection.hub.start().done(function () {
        //vm.connected(true);
    });*/

    //hub.server.OnRefresh();


    if (document.cookie.indexOf('checkRefresh') === -1) {
        // cookie doesn't exist, create it now
        document.cookie = 'checkRefresh=1';
    }
    else {
        // not first visit, so alert
        //alert('You refreshed!');
        console.log('You refreshed!');


    }
}

function onNodeClick(object) {

    console.log("onNodeClick");
    console.log(object);

    $.connection.hub.url = "http://localhost:15123/signalr";
    var hub = $.connection.MyHub;

    console.log(object.getAttribute('customer'));
    hub.server.nodeClick(object.id, object.textContent, object.getAttribute('customer'));
}

function onSwitchClick() {
    console.log("onSwitchClick");

    $.connection.hub.url = "http://localhost:15123/signalr";
    var hub = $.connection.MyHub;

    hub.server.onSwitchClick();
}