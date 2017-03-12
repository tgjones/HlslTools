/*---------------------------------------------------------
 * Copyright (C) Microsoft Corporation. All rights reserved.
 *--------------------------------------------------------*/

'use strict';

import vscode = require('vscode');

export interface IDeveloperSettings {
    editorServicesLogLevel?: string;
}

export interface ISettings {
    developer?: IDeveloperSettings;
}

export function load(myPluginId: string): ISettings {
    let configuration: vscode.WorkspaceConfiguration = vscode.workspace.getConfiguration(myPluginId);

    let defaultDeveloperSettings: IDeveloperSettings = {
        editorServicesLogLevel: "Normal"
    };

    return {
        developer: configuration.get<IDeveloperSettings>("developer", defaultDeveloperSettings)
    };
}
