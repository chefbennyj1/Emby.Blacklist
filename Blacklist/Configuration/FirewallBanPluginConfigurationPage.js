define(["require", "loading", "dialogHelper", "formDialogStyle", "emby-checkbox", "emby-select", "emby-toggle"],
    function(require, loading, dialogHelper) {

        var pluginId = "81C4097F-BE07-48B6-B76E-48691FDFC2C5";

        ApiClient.deleteFirewallRule = function(ip, ruleName) {
            var url = this.getUrl("DeleteFirewallRule?Ip=" + ip + "&RuleName=" + ruleName);
            return this.ajax({
                type: "DELETE",
                url: url
            });
        };

        function openSettingsDialog() {

            var dlg = dialogHelper.createDialog({
                size: "medium-tall",
                removeOnClose: !1,
                scrollY: true
            });

            dlg.classList.add("formDialog");
            dlg.classList.add("ui-body-a");
            dlg.classList.add("background-theme-a");
            dlg.style.maxWidth = "35%";
            dlg.style.maxHeight = "80%";

            var html = '';

            html += '<div class="formDialogHeader" style="display:flex">';
            html += '<button is="paper-icon-button-light" class="btnCloseDialog autoSize paper-icon-button-light" tabindex="-1"><i class="md-icon">arrow_back</i></button><h3 class="formDialogHeaderTitle">Settings</h3>';
            html += '</div>';

            html += '<div class="formDialogContent" style="margin:2em">';
            html += '<div class="scrollY dialogContentInner" style="max-height: 42em;">';
            html += '<div style="flex-grow:1;">';

            html += '<div class="paperList" style="padding:2em">';
            html += '<div class="inputContainer" style="display: flex">';
            html += '<label style="width: auto;" class="emby-toggle-label">';
            html += '<input is="emby-toggle" type="checkbox" id="enableLookup" class="noautofocus emby-toggle emby-toggle-focusring">';
            html += '<span class="toggleLabel">Enable GeoIp Location</span>';
            html += '</label>';
            html += '</div> ';
            html += '</div>'; 

            html += '<div class="paperList" style="padding:2em; margin-top:3em">';
            html += '<div class="inputContainer fldFailedAttempts">';
            html += '<label class="inputLabel inputLabelUnfocused" for="txtFailedLoginAttemptLimit">Failed login attempt limit:</label>';
            html += '<input type="number" id="txtFailedLoginAttemptLimit" pattern="[0-9]*" min="3" step="1" label="Failed login attempt limit:" class="emby-input">';
            html += '<div class="fieldDescription">A limit for all user login attempts in 30 seconds. Default is 3 attempts before ban.';
            html += '</div>';
            html += '</div>';
            html += '</div>';

            html += '<div class="paperList" style="padding:2em; margin-top:3em">';
            html += '<div class="inputContainer" style="display: flex">';
            html += '<label style="width: auto;" class="emby-toggle-label">';
            html += '<input is="emby-toggle" type="checkbox" id="ignoreInternalLoginAttempts" class="noautofocus emby-toggle emby-toggle-focusring">';
            html += '<span class="toggleLabel">Ignore Internal Network Failed Login Attempts</span>';
            html += '</label>';
            html += '</div>';
            html += '</div>';
             
            html += '</div>';
             
            html += '<div class="formDialogFooter" style="padding-top:2.5em">';
            html += '<button id="btnSave" class="raised button-submit block emby-button" is="emby-button">Save</button>';
            html += '</div>';

          
            html += '</div>';
            html += '</div>';

            dlg.innerHTML = html;

            

            //Enable toggle state
            ApiClient.getPluginConfiguration(pluginId).then((config) => {
                if (config.EnableGeoIp) {
                    dlg.querySelector('#enableLookup').checked = config.EnableGeoIp;
                } else {
                    dlg.querySelector('#enableLookup').checked = false;
                }

                if (config.ConnectionAttemptsBeforeBan) {
                    dlg.querySelector('#txtFailedLoginAttemptLimit').value = config.ConnectionAttemptsBeforeBan;
                }

                if (config.IgnoreInternalFailedLoginAttempts) {
                    dlg.querySelector('#ignoreInternalLoginAttempts').checked = config.IgnoreInternalFailedLoginAttempts;
                } else {
                    dlg.querySelector('#ignoreInternalLoginAttempts').checked = false;
                } 
            });

            dlg.querySelector('#ignoreInternalLoginAttempts').addEventListener('change', (e) => {
                ApiClient.getPluginConfiguration(pluginId).then((config) => {
                    config.IgnoreInternalFailedLoginAttempts = e.target.checked;
                    ApiClient.updatePluginConfiguration(pluginId, config).then((result) => {
                        Dashboard.processPluginConfigurationUpdateResult(result);
                    });
                });
            });

            dlg.querySelector('#txtFailedLoginAttemptLimit').addEventListener('change',
                () => {
                    ApiClient.getPluginConfiguration(pluginId).then((config) => {
                        config.ConnectionAttemptsBeforeBan =
                            dlg.querySelector('#txtFailedLoginAttemptLimit').value;
                        ApiClient.updatePluginConfiguration(pluginId, config).then(() => {});
                    });
                });

            dlg.querySelector('#enableLookup').addEventListener('change',
                () => {
                    ApiClient.getPluginConfiguration(pluginId).then((config) => {
                        config.EnableGeoIp = dlg.querySelector('#enableLookup').checked;
                        ApiClient.updatePluginConfiguration(pluginId, config).then(() => {});
                    });
                });

            dlg.querySelector('#btnSave').addEventListener('click',
                () => {
                    ApiClient.getPluginConfiguration(pluginId).then((config) => {
                        

                        ApiClient.updatePluginConfiguration(pluginId, config).then(() => {});
                        dialogHelper.close(dlg);
                    });

                });

            dlg.querySelector('.btnCloseDialog').addEventListener('click',
                () => {
                    dialogHelper.close(dlg);
                });

            dialogHelper.open(dlg);

        }

        function openGlobeDialog(connections) {
            var dlg = dialogHelper.createDialog({
                size: "medium-tall",
                removeOnClose: !1,
                scrollY: true
            });

            dlg.classList.add("formDialog");
            dlg.classList.add("ui-body-a");
            dlg.classList.add("background-theme-a");
            dlg.style.maxWidth = "35%";
            dlg.style.maxHeight = "80%";

            var connection = connections[0];

            var html = '';
            html += '<div class="formDialogHeader" style="display:flex">';
            html += '<button is="paper-icon-button-light" class="btnCloseDialog autoSize paper-icon-button-light" tabindex="-1"><i class="md-icon">arrow_back</i></button><h3 class="formDialogHeaderTitle">Settings</h3>';
            html += '</div>';

            html += '<div class="formDialogContent" style="margin:2em">';
            html += '<div class="dialogContentInner" style="max-height: 42em;">';
            html += '<div style="flex-grow:1;">';

            html += '<div id="connectionData">';
           
            

            html += '</div>';
            html += '</div>';
            html += '</div>';

            dlg.innerHTML = html;
            dialogHelper.open(dlg); 
            
        }

        function getBannedConnectionTableHtml(config) {
            var html = '';
            config.BannedConnections.forEach(connection => {
                html += '<tr class="detailTableBodyRow detailTableBodyRow" id="' + connection.Id + '">';
                html += '<td class="detailTableBodyCell fileCell"></td>';
                html += '<td data-title="Date" class="detailTableBodyCell fileCell">' + new Date(Date.parse(connection.BannedDateTime)).toDateString() + '</td>';
                html += '<td data-title="Country" class="detailTableBodyCell fileCell"><img style="width:3em" src=\"' + connection.FlagIconUrl + '\"></td>';
                html += '<td data-title="Firewall Rule Name" class="detailTableBodyCell fileCell">' + connection.RuleName + '</td>';
                html += '<td data-title="Name" class="detailTableBodyCell fileCell">' + connection.UserAccountName + '</td>';
                html += '<td data-title="Ip" class="detailTableBodyCell fileCell">' + connection.Ip + '</td>';
                html += '<td data-title="Isp" class="detailTableBodyCell fileCell">' + connection.ServiceProvider + '</td>';
                html += '<td data-title="Ip" class="detailTableBodyCell fileCell">' + connection.Proxy + '</td>';
                html += '<td data-title="Device" class="detailTableBodyCell fileCell">' + connection.DeviceName + '</td>';
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
                 
                    var table = view.querySelector(".tblFirewallBanResults");

                    ApiClient._webSocket.addEventListener('message',
                        function (msg) {
                            var json = JSON.parse(msg.data);
                            if (json.MessageType === "FirewallAdded") {
                                ApiClient.getPluginConfiguration(pluginId).then((config) => {

                                    if (config.BannedConnections) {

                                        view.querySelector('.connectionTableResultBody').innerHTML =
                                            getBannedConnectionTableHtml(config);

                                        view.querySelectorAll('.deleteRule').forEach(button => {
                                            button.addEventListener('click',
                                                (e) => {
                                                    deleteButtonPress(e, view).then((result) => {
                                                        Dashboard.processPluginConfigurationUpdateResult(
                                                            result);
                                                    });
                                                });
                                        });

                                        //view.querySelectorAll('td[data-title="Country"]').forEach(flag => {
                                        //    flag.addEventListener('click', (e) => {
                                        //        e.preventDefault();
                                        //        var connectionId = e.target.closest('tr').id;
                                        //        var connection = config.BannedConnections.filter(c => c.Id === connectionId);
                                        //        openGlobeDialog(connection);
                                        //    });
                                        //});
                                    }
                                });
                            }
                        });

                    ApiClient.getPluginConfiguration(pluginId).then((config) => {

                        if (config.EnableFirewallBlock) {
                            view.querySelector('#enableFirewallBlock').checked = config.EnableFirewallBlock;
                            config.EnableFirewallBlock ? table.style = 'display:block' : 'display:none';

                        } else {
                            view.querySelector('#enableFirewallBlock').checked = false;
                            table.style = 'display:none';
                        }

                       

                        if (config.BannedConnections) {

                            view.querySelector('.connectionTableResultBody').innerHTML =
                                getBannedConnectionTableHtml(config);

                            view.querySelectorAll('.deleteRule').forEach(button => {
                                button.addEventListener('click',
                                    (e) => {
                                        deleteButtonPress(e, view).then((result) => {
                                            Dashboard.processPluginConfigurationUpdateResult(result);
                                        });
                                    });
                            });

                            //view.querySelectorAll('td[data-title="Country"]').forEach(flag => {
                            //    flag.addEventListener('click', (e) => {
                            //        e.preventDefault();
                            //        var connectionId = e.target.closest('tr').id;
                            //        var connection = config.BannedConnections.filter(c => c.Id === connectionId);
                            //        openGlobeDialog(connection);
                            //    });
                            //});

                        }

                    });

                    view.querySelector('#openSettingsDialog').addEventListener('click',
                        (e) => {
                            e.preventDefault();
                            openSettingsDialog();
                        });

                    //view.querySelector('#openGlobeDialog').addEventListener('click',
                    //    (e) => {
                    //        e.preventDefault();
                    //        ApiClient.getPluginConfiguration(pluginId).then((config) => {
                    //            openGlobeDialog(config.BannedConnections[0]);
                    //        });
                    //    });
                    
                    view.querySelector('#enableFirewallBlock').addEventListener('change',
                        (e) => {
                            ApiClient.getPluginConfiguration(pluginId).then((config) => {
                                config.EnableFirewallBlock = e.target.checked;
                                ApiClient.updatePluginConfiguration(pluginId, config).then((result) => {
                                    Dashboard.processPluginConfigurationUpdateResult(result);
                                    e.target.checked === true
                                        ? table.style = 'display: block'
                                        : 'display: none';
                                });
                            });
                        });
                });

              
        }

        
    });