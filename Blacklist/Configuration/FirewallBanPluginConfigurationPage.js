define(["require", "loading", "dialogHelper", "formDialogStyle", "emby-checkbox", "emby-select", "emby-toggle"],
    function (require, loading, dialogHelper) {

        var pluginId = "81C4097F-BE07-48B6-B76E-48691FDFC2C5";

        ApiClient.deleteFirewallRule = function (ip, ruleName) {
            var url = this.getUrl("DeleteFirewallRule?Ip=" + ip + "&RuleName=" + ruleName);
            return this.ajax({
                type: "DELETE",
                url: url
            });
        };                            
           
        function getBannedConnectionTableHtml(config) {
            var html = '';
            config.BannedConnections.forEach(connection => {
                html += '<tr class="detailTableBodyRow detailTableBodyRow" id="' + connection.Id + '">';
                html += '<td class="detailTableBodyCell fileCell"></td>';
                html += '<td data-title="Firewall Rule Name" class="detailTableBodyCell fileCell">' + connection.RuleName + '</td>';
                html += '<td data-title="Name" class="detailTableBodyCell fileCell">' + connection.UserAccountName + '</td>';
                html += '<td data-title="Ip" class="detailTableBodyCell fileCell">' + connection.Ip + '</td>';
                html += '<td data-title="Device" class="detailTableBodyCell fileCell">' + connection.DeviceName + '</td>';
                html += '<td data-title="Date" class="detailTableBodyCell fileCell">' + new Date(Date.parse(connection.BannedDateTime)).toDateString() + '</td>';
                html += '<td data-title="Remove" class="detailTableBodyCell fileCell"><button class="fab deleteRule emby-button"><i class="md-icon">clear</i></button></td>';
                html += '</tr>';
            });
            return html;
        }

        function deleteButtonPress(event, view) {
            return new Promise((resolve, reject) => {

                var row      = event.target.closest('tr');
                var ip       = row.querySelector('[data-title="Ip"]').innerHTML;
                var ruleName = row.id;

                ApiClient.deleteFirewallRule(ip, ruleName).then((response) => {
                    if (response.statusText === "OK") {
                        ApiClient.getPluginConfiguration(pluginId).then((c) => {
                            c.BannedConnections = c.BannedConnections.filter(connection => connection.id !== id);
                            ApiClient.updatePluginConfiguration(pluginId, c).then((result) => {
                                view.querySelector('.connectionTableResultBody').innerHTML = getBannedConnectionTableHtml(c);
                                resolve(result);
                            });
                        });
                    } else {
                        reject('error');
                    }
                    
                });
            });
        }

        return function(view) {

            view.addEventListener('viewshow',
                () => {

                    ApiClient._webSocket.addEventListener('message', function (msg) {
                        var json = JSON.parse(msg.data);
                        if (json.MessageType === "FirewallAdded") {
                            ApiClient.getPluginConfiguration(pluginId).then((config) => {

                               
                                if (config.BannedConnections) {

                                    view.querySelector('.connectionTableResultBody').innerHTML = getBannedConnectionTableHtml(config);

                                    view.querySelectorAll('.deleteRule').forEach(button => {
                                        button.addEventListener('click',
                                            (e) => {
                                                deleteButtonPress(e, view).then((result) => {
                                                    Dashboard.processPluginConfigurationUpdateResult(result);
                                                });
                                            });
                                    });
                                }
                            });
                        }

                    });

                    ApiClient.getPluginConfiguration(pluginId).then((config) => {

                        if (config.EnableFirewallBlock) {
                            view.querySelector('#enableFirewallBlock').checked = config.EnableFirewallBlock;
                        } else {
                            view.querySelector('#enableFirewallBlock').checked = false;
                        }

                        if (config.BannedConnections) {
                            
                            view.querySelector('.connectionTableResultBody').innerHTML = getBannedConnectionTableHtml(config);

                            view.querySelectorAll('.deleteRule').forEach(button => {
                                button.addEventListener('click',
                                    (e) => {
                                        deleteButtonPress(e, view).then((result) => {
                                            Dashboard.processPluginConfigurationUpdateResult(result);
                                        });
                                    });
                            });

                        }

                        if (config.ConnectionAttemptsBeforeBan) {
                            view.querySelector('#txtFailedLoginAttemptLimit').value =
                                config.ConnectionAttemptsBeforeBan;
                        }   

                    });

                    view.querySelector('#enableFirewallBlock').addEventListener('change', () => {
                        ApiClient.getPluginConfiguration(pluginId).then((config) => {
                            config.EnableFirewallBlock = view.querySelector('#enableFirewallBlock').checked;
                            ApiClient.updatePluginConfiguration(pluginId, config).then((result) => {
                                Dashboard.processPluginConfigurationUpdateResult(result);
                            });
                        });
                    }); 
                   
                    view.querySelector('#txtFailedLoginAttemptLimit').addEventListener('change',
                        () => {
                            ApiClient.getPluginConfiguration(pluginId).then((config) => {
                                config.ConnectionAttemptsBeforeBan =
                                    view.querySelector('#txtFailedLoginAttemptLimit').value;
                                ApiClient.updatePluginConfiguration(pluginId, config).then(() => {});
                            });
                        });
                            
                });
        }
    });