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

        function createConnectionTableHtml(bannedConnections) {
            var html = '';
            bannedConnections.forEach(connection => {
                html += '<tr class="detailTableBodyRow detailTableBodyRow" id="' + connection.Id + '">';
                html += '<td class="detailTableBodyCell fileCell"></td>';
                html += '<td data-title="Name" class="detailTableBodyCell fileCell">' + connection.RuleName + '</td>';
                html += '<td data-title="Ip" class="detailTableBodyCell fileCell">' + connection.Ip + '</td>';
                html += '<td data-title="DateTime" class="detailTableBodyCell fileCell">' +  connection.BannedDateTime + '</td>';
                html += '<td data-title="Remove" class="detailTableBodyCell fileCell"><div class="deleteRule"><i class="md-icon">delete</i></div></td>';
                html += '</tr>';
            });
            return html;
        }

        return function(view) {

            view.addEventListener('viewshow',
                () => {
                    ApiClient.getPluginConfiguration(pluginId).then((config) => {
                        if (config.BannedConnections) {
                            var firewallBanTableResultBody = view.querySelector('.firewallBanTableResultBody');
                            firewallBanTableResultBody.innerHTML = createConnectionTableHtml(config.BannedConnections);

                            var ruleDeleteButtons = view.querySelectorAll('.deleteRule');
                            ruleDeleteButtons.forEach(button => {
                                button.addEventListener('click',
                                    (e) => {

                                        var row = e.target.closest('tr');
                                        var ip = row.querySelector('[data-title="Ip"]').innerHTML;
                                        var id = row.id;

                                        ApiClient.deleteFirewallRule(ip, id).then((response) => {
                                            if (response.statusText === "OK") {
                                                ApiClient.getPluginConfiguration(pluginId).then((config) => {
                                                    var filteredBannedConnection = config.BannedConnections.filter(connection => connection.id !== id);
                                                    config.BannedConnections = filteredBannedConnection;
                                                    ApiClient.updatePluginConfiguration(pluginId, config).then(() => {
                                                        firewallBanTableResultBody.innerHTML = createConnectionTableHtml(config.BannedConnections);
                                                    });
                                                });
                                            };
                                        });
                                    });
                            }); 
                        }
                        if (config.ConnectionAttemptsBeforeBan) {
                            view.querySelector('#txtFailedLoginAttemptLimit').value =
                                config.ConnectionAttemptsBeforeBan;
                        }
                        if (config.BanDurationMinutes) {
                            view.querySelector('#txtBlockRuleTimeLimit').value = config.BanDurationMinutes;
                        }
                    });

                    view.querySelector('#txtFailedLoginAttemptLimit').addEventListener('change',
                        () => {
                            ApiClient.getPluginConfiguration(pluginId).then((config) => {
                                config.ConnectionAttemptsBeforeBan =
                                    view.querySelector('#txtFailedLoginAttemptLimit').value;
                                ApiClient.updatePluginConfiguration(pluginId, config).then(() => {});
                            });
                        });

                    view.querySelector('#txtBlockRuleTimeLimit').addEventListener('change',
                        () => {
                            ApiClient.getPluginConfiguration(pluginId).then((config) => {
                                config.BanDurationMinutes  = view.querySelector('#txtBlockRuleTimeLimit').value;
                                ApiClient.updatePluginConfiguration(pluginId, config).then(() => { });
                            });
                        });

                });
        }
    });