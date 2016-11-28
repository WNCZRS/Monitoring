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

    hub.client.machineInitialize = function (MACaddress) {

        var matchMAC = ko.utils.arrayFirst(vm.propValues(), function (item) {
            return item.propName() === propName;
        }); 

        if (!matchMAC) 
            vm.propValues.push(ko.mapping.fromJS(item));
    }



    hub.client.pluginsMessage = function (clientOutput) {

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
            //headRow.className = "header";
            //headRow.setAttribute("data-toggle", "collapse");
            //headRow.setAttribute("data-target", "#collapseElem");
            result.appendChild(headRow);

            plugin.PluginOutputList.forEach(function (pluginElement) {
                var row = document.createElement('tr');
                var cellName = document.createElement('td');
                var cellValue = document.createElement('td');
                cellName.textContent = pluginElement.PropertyName;
                cellValue.textContent = pluginElement.Value;
                row.appendChild(cellName);
                row.appendChild(cellValue);
                //row.id = "collapseElem";
                result.appendChild(row);
            });
        });
        result.id = clientOutput.ID;

        if (document.getElementById(clientOutput.ID) === null) {
            document.body.appendChild(result);
        }
        else {
            document.body.replaceChild(result, document.getElementById(clientOutput.ID));
        }
    }

    // Start the connectio
    $.connection.hub.start().done(function () {
        vm.connected(true);
    });
});