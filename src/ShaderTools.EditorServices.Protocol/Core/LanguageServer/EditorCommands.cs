//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using ShaderTools.EditorServices.Protocol.MessageProtocol;

namespace ShaderTools.EditorServices.Protocol.LanguageServer
{
    public class ClientEditorContext
    {
        public string CurrentFilePath { get; set; }

        public Position CursorPosition { get; set; }

        public Range SelectionRange { get; set; }

    }

    public class GetEditorContextRequest
    {
        public static readonly
            RequestType<GetEditorContextRequest, ClientEditorContext> Type =
            RequestType<GetEditorContextRequest, ClientEditorContext>.Create("editor/getEditorContext");
    }

    public enum EditorCommandResponse
    {
        Unsupported,
        OK
    }

    public class InsertTextRequest
    {
        public static readonly
            RequestType<InsertTextRequest, EditorCommandResponse> Type =
            RequestType<InsertTextRequest, EditorCommandResponse>.Create("editor/insertText");

        public string FilePath { get; set; }

        public string InsertText { get; set; }

        public Range InsertRange { get; set; }
    }

    public class SetSelectionRequest
    {
        public static readonly
            RequestType<SetSelectionRequest, EditorCommandResponse> Type =
            RequestType<SetSelectionRequest, EditorCommandResponse>.Create("editor/setSelection");

        public Range SelectionRange { get; set; }
    }

    public class SetCursorPositionRequest
    {
        public static readonly
            RequestType<SetCursorPositionRequest, EditorCommandResponse> Type =
            RequestType<SetCursorPositionRequest, EditorCommandResponse>.Create("editor/setCursorPosition");

        public Position CursorPosition { get; set; }
    }

    public class OpenFileRequest
    {
        public static readonly
            RequestType<string, EditorCommandResponse> Type =
            RequestType<string, EditorCommandResponse>.Create("editor/openFile");
    }

    public class CloseFileRequest
    {
        public static readonly
            RequestType<string, EditorCommandResponse> Type =
            RequestType<string, EditorCommandResponse>.Create("editor/closeFile");
    }

    public class ShowInformationMessageRequest
    {
        public static readonly
            RequestType<string, EditorCommandResponse> Type =
            RequestType<string, EditorCommandResponse>.Create("editor/showInformationMessage");
    }

    public class ShowWarningMessageRequest
    {
        public static readonly
            RequestType<string, EditorCommandResponse> Type =
            RequestType<string, EditorCommandResponse>.Create("editor/showWarningMessage");
    }

    public class ShowErrorMessageRequest
    {
        public static readonly
            RequestType<string, EditorCommandResponse> Type =
            RequestType<string, EditorCommandResponse>.Create("editor/showErrorMessage");
    }

    public class SetStatusBarMessageRequest
    {
        public static readonly
            RequestType<StatusBarMessageDetails, EditorCommandResponse> Type =
            RequestType<StatusBarMessageDetails, EditorCommandResponse>.Create("editor/setStatusBarMessage");
    }

    public class StatusBarMessageDetails
    {
        public string Message { get; set; }

        public int? Timeout { get; set; }
    }
}

