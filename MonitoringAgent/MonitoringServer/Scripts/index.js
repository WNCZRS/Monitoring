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
    var hub = $.connection.pluginInfo;


    hub.client.initMessage = function (clientOutput) {
        console.log("initMessage");

        var ul3 = document.createElement('ul');
        var li3 = document.createElement('li');
        var span3 = document.createElement('span');
        span3.className = "leaf";
        span3.setAttribute("onclick", "onNodeClick(this)");
        span3.textContent = clientOutput.PCName;
        span3.id = clientOutput.ID;
        li3.appendChild(span3);
        ul3.appendChild(li3);

        if (clientOutput.Customer === null || clientOutput.Customer === "") {
            if (document.getElementById(clientOutput.ID) === null) {
                document.getElementById("noCategory").appendChild(li3);
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
            document.getElementById("rootNode").appendChild(li2);
            if (document.getElementById(clientOutput.ID) === null) {
                document.getElementById(clientOutput.Customer).appendChild(li3);
            }
        } else if (document.getElementById(clientOutput.ID) === null) {
            document.getElementById(clientOutput.Customer).appendChild(li3);
        }
    }

    hub.client.pluginsMessage = function (clientOutput) {
        console.log("pluginsMessage");

        var result = document.createElement('table');
        var pcName = document.createElement('tr');
        var nameCell = document.createElement('th');
        nameCell.textContent = clientOutput.PCName;
        pcName.appendChild(nameCell);
        result.appendChild(pcName);

        clientOutput.CollectionList.forEach(function (plugin) {
            var headRow = document.createElement('tr');
            var headCell = document.createElement('th');
            headCell.textContent = plugin.PluginName;
            headCell.setAttribute("colspan", "2");
            headRow.appendChild(headCell);
            result.appendChild(headRow);

            plugin.PluginOutputList.forEach(function (pluginElement) {
                var row = document.createElement('tr');
                var cellName = document.createElement('td');
                var cellValue = document.createElement('td');
                cellName.textContent = pluginElement.PropertyName;
                cellValue.textContent = pluginElement.Value;
                if (pluginElement.IsCritical) {
                    cellValue.className = "alertRow";
                }
                row.appendChild(cellName);
                row.appendChild(cellValue);
                result.appendChild(row);
            });
        });
        result.id = "resultTable";

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

    // Start the connectio
    $.connection.hub.start().done(function () {
        vm.connected(true);
    });
});

function onNodeClick(object) {

    //console.log(object);

   /* var ObjectData = {
        "id": object.id
    }

    var JsonData = JSON.stringify(ObjectData, null, 4);
    var serviceUrl = "api/Plugin/nodeSelected/";
    var method = "POST";

    $.ajax({
        type: method,
        url: serviceUrl,
        contentType: "application/json; charset=utf-8",
        data: JsonData,
        //headers: { "Authorization": "Bearer " + app.dataModel.getAccessToken() }
    }).done(function () {
        console.info("succes");
    }).fail(function () {
        console.error("fail");
    });
    */
}