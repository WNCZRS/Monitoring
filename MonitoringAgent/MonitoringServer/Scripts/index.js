$(function () {

    // The view model that is bound to our view
    var ViewModel = function () {
        var self = this;

        // Whether we're connected or not
        self.connected = ko.observable(false);
        //self.connected = plug.observable(false);

        // Collection of machines that are connected
        self.propValues = ko.observableArray();
        //self.plugName = plug.observableArray();
    };

    // Instantiate the viewmodel..
    var vm = new ViewModel();

    // .. and bind it to the view
    ko.applyBindings(vm, $("#computerInfo")[0]);
    //plug.applyBindings(vm, $("#computerInfo")[0]);

    // Get a reference to our hub
    var hub = $.connection.pluginInfo;

    // Add a handler to receive updates from the server
    hub.client.pluginMessage = function (propName, value) {
       
        var propValues = {
            propName: propName,
            value: value
        }

        var pluginModel = ko.mapping.fromJS(propValues);

        /*var matchPlug = plug.utils.arrayFirst(vm.plugName(), function (item) {
            return item.plugName() == plugName;
        });

        if (!matchPlug)
            vm.plugName.push(plugName);
        else {
            var index = vm.plugName.indexOf(matchPlug);
            vm.plugName.replace(vm.plugName()[index], plugName)
        }*/

        var match = ko.utils.arrayFirst(vm.propValues(), function (item) {
            return item.propName() == propName;
        });  

        if (!match)
            vm.propValues.push(pluginModel);
        else {
            var index = vm.propValues.indexOf(match);
            vm.propValues.replace(vm.propValues()[index], pluginModel);
        }
    }

    // Start the connectio
    $.connection.hub.start().done(function () {
        vm.connected(true);
    });
});