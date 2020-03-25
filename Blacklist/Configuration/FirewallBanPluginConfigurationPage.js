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
                html += '<td data-title="DateTime" class="detailTableBodyCell fileCell">' + connection.BannedDateTime + '</td>';
                
                html += '<td data-title="Location" class="detailTableBodyCell fileCell"><img style="height:1em" src="' + connection.LookupData.location.country_flag + '"/> '  + connection.LookupData.city + ", " + connection.LookupData.region_name + ", " + connection.LookupData.country_name + '</td>';
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
                                        var ip  = row.querySelector('[data-title="Ip"]').innerHTML;
                                        var id  = row.id;

                                        ApiClient.deleteFirewallRule(ip, id).then((response) => {
                                            if (response.statusText === "OK") {
                                                ApiClient.getPluginConfiguration(pluginId).then((config) => {
                                                    config.BannedConnections = config.BannedConnections.filter(connection => connection.id !== id);
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