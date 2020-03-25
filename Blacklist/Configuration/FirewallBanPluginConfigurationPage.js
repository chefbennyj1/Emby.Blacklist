define(["require", "loading", "dialogHelper", "emby-checkbox", "emby-select"],
    function (require, loading, dialogHelper) {

        var pluginId = "81C4097F-BE07-48B6-B76E-48691FDFC2C5";

        ApiClient.deleteFirewallRule = function (ip, ruleName) {
            var url = this.getUrl("DeleteFirewallRule?Ip=" + ip + "&RuleName=" + ruleName);
            return this.ajax({
                type: "DELETE",
                url: url
            });
        };                            

        function getConnectionTableHtml(bannedConnections) {
            var html = '';
            bannedConnections.forEach(connection => {
                html += '<tr class="detailTableBodyRow detailTableBodyRow" id="' + connection.Id + '">';
                html += '<td class="detailTableBodyCell fileCell"></td>';
                html += '<td data-title="Name" class="detailTableBodyCell fileCell">' + connection.RuleName + '</td>';
                html += '<td data-title="Ip" class="detailTableBodyCell fileCell">' + connection.Ip + '</td>';
                html += '<td data-title="DateTime" class="detailTableBodyCell fileCell">' + connection.BannedDateTime + '</td>';
                if (connection.LookupData.location) {
                    html +=
                        '<td data-title="Location" class="detailTableBodyCell fileCell"><img style="height:1em" src="' +
                        connection.LookupData.location.country_flag +
                        '"/></td>';
                    html += '<td data-title="Location" class="detailTableBodyCell fileCell">' +
                        connection.LookupData.city +
                        ", " +
                        connection.LookupData.region_name +
                        ", " +
                        connection.LookupData.country_name +
                        '</td>';
                } else {
                    html += '<td data-title="Location" class="detailTableBodyCell fileCell">Internal</td>';
                    html += '<td data-title="Location" class="detailTableBodyCell fileCell">Subnet Address</td>';
                }
                html += '<td data-title="Remove" class="detailTableBodyCell fileCell"><div class="deleteRule"><i class="md-icon">delete</i></div></td>';
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
                                view.querySelector('.connectionTableResultBody').innerHTML =
                                    getConnectionTableHtml(c.BannedConnections);
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

                                    view.querySelector('.connectionTableResultBody').innerHTML = getConnectionTableHtml(config.BannedConnections);

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

                        if (config.BannedConnections) {
                            
                            view.querySelector('.connectionTableResultBody').innerHTML = getConnectionTableHtml(config.BannedConnections);

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
                        if (config.EnableReverseLookup) {
                            config.EnableReverseLookup = view.querySelector('#enableReverseLookup').checked = config.EnableReverseLookup;
                            if (config.EnableReverseLookup === true) {
                                if (view.querySelector('.fldIpStackApiKey ').classList.contains('hide'))
                                    view.querySelector('.fldIpStackApiKey ').classList.remove('hide'); 
                            } else {
                                if (!view.querySelector('.fldIpStackApiKey ').classList.contains('hide'))
                                    view.querySelector('.fldIpStackApiKey ').classList.add('hide');
                            }
                            if (config.IpStackApiKey) {
                                view.querySelector('#txtIpStackApiKey').value = config.IpStackApiKey;
                            }
                        }
                    });

                    view.querySelector('#enableReverseLookup').addEventListener('change', () => {
                        var reverseLookup = view.querySelector('#enableReverseLookup');
                        switch (reverseLookup.checked) {
                            case true:
                                if (view.querySelector('.fldIpStackApiKey ').classList.contains('hide'))
                                    view.querySelector('.fldIpStackApiKey ').classList.remove('hide'); 
                                break;
                            case false:
                                if(!view.querySelector('.fldIpStackApiKey ').classList.contains('hide'))
                                    view.querySelector('.fldIpStackApiKey ').classList.add('hide');
                                break;
                        }   
                    });

                    view.querySelector('#saveIpStackApiKey').addEventListener('click', () => {
                        ApiClient.getPluginConfiguration(pluginId).then((config) => {
                            config.IpStackApiKey = view.querySelector('#txtIpStackApiKey').value;
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