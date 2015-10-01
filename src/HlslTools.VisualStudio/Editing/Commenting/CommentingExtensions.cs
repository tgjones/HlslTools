// Based on https://github.com/Microsoft/nodejstools/blob/1144d653a43b4048ac390bb46a7e255a674c451c/Nodejs/Product/Nodejs/Editor/EditorExtensions.cs
// Original licence follows:

//*********************************************************//
//    Copyright (c) Microsoft. All rights reserved.
//    
//    Apache 2.0 License
//    
//    You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
//    
//    Unless required by applicable law or agreed to in writing, software 
//    distributed under the License is distributed on an "AS IS" BASIS, 
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or 
//    implied. See the License for the specific language governing 
//    permissions and limitations under the License.
//
//*********************************************************//

using System.Diagnostics;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace HlslTools.VisualStudio.Editing.Commenting
{
    internal static class CommentingExtensions
    {
        public static bool CommentOrUncommentBlock(this ITextView view, bool comment)
        {
            SnapshotPoint start, end;
            SnapshotPoint? mappedStart, mappedEnd;

            // select the region to comment.
            // if the selected area is non-empty, let's comment the group of lines, 
            // otherwise just comment the current line
            if (view.Selection.IsActive && !view.Selection.IsEmpty)
            {
                // comment every line in the selection
                start = view.Selection.Start.Position;
                end = view.Selection.End.Position;
                mappedStart = MapPoint(view, start);

                var endLine = end.GetContainingLine();

                // If we grabbed the last line by just the start, don't comment it as it isn't actually selected.
                if (endLine.Start == end)
                {
                    end = end.Snapshot.GetLineFromLineNumber(endLine.LineNumber - 1).End;
                }

                mappedEnd = MapPoint(view, end);
            }
            else
            {
                // comment the current line
                start = end = view.Caret.Position.BufferPosition;
                mappedStart = mappedEnd = MapPoint(view, start);
            }

            // Now that we have selected the region to comment, let's do the work.
            if (mappedStart != null && mappedEnd != null && mappedStart.Value <= mappedEnd.Value)
            {
                if (comment)
                {
                    CommentRegion(view, mappedStart.Value, mappedEnd.Value);
                }
                else
                {
                    UncommentRegion(view, mappedStart.Value, mappedEnd.Value);
                }

                // After commenting, update the selection to the complete commented area.
                if (view.TextBuffer.IsHlslContent())
                {
                    UpdateSelection(view, start, end);
                }
                return true;
            }

            return false;
        }

        private static SnapshotPoint? MapPoint(ITextView view, SnapshotPoint point)
        {
            return view.BufferGraph.MapDownToFirstMatch(
                point,
                PointTrackingMode.Positive,
                IsHlslContent,
                PositionAffinity.Successor);
        }

        /// <summary>
        /// Adds comment characters (//) to the start of each line.  If there is a selection the comment is applied
        /// to each selected line.  Otherwise the comment is applied to the current line.
        /// </summary>
        private static void CommentRegion(ITextView view, SnapshotPoint start, SnapshotPoint end)
        {
            Debug.Assert(start.Snapshot == end.Snapshot);
            var snapshot = start.Snapshot;

            using (var edit = snapshot.TextBuffer.CreateEdit())
            {
                int minColumn = int.MaxValue;

                // First pass, determine the position to place the comment.
                // This is done as we want all of the comment lines to line up at the end.
                // We also should ignore whitelines in this calculation.
                for (int i = start.GetContainingLine().LineNumber; i <= end.GetContainingLine().LineNumber; i++)
                {
                    var curLine = snapshot.GetLineFromLineNumber(i);
                    var text = curLine.GetText();

                    int firstNonWhitespace = IndexOfNonWhitespaceCharacter(text);
                    if (firstNonWhitespace >= 0 && firstNonWhitespace < minColumn)
                    {
                        minColumn = firstNonWhitespace;
                    }
                }

                // Second pass, place the comment.
                for (int i = start.GetContainingLine().LineNumber; i <= end.GetContainingLine().LineNumber; i++)
                {
                    var curLine = snapshot.GetLineFromLineNumber(i);
                    if (string.IsNullOrWhiteSpace(curLine.GetText()))
                    {
                        continue;
                    }

                    Debug.Assert(curLine.Length >= minColumn);

                    edit.Insert(curLine.Start.Position + minColumn, "//");
                }

                edit.Apply();
            }
        }

        private static int IndexOfNonWhitespaceCharacter(string text)
        {
            for (int j = 0; j < text.Length; j++)
            {
                if (!char.IsWhiteSpace(text[j]))
                {
                    return j;
                }
            }
            return -1;
        }

        /// <summary>
        /// Removes a comment character (//) from the start of each line.  If there is a selection the character is
        /// removed from each selected line.  Otherwise the character is removed from the current line.  Uncommented
        /// lines are ignored.
        /// </summary>
        private static void UncommentRegion(ITextView view, SnapshotPoint start, SnapshotPoint end)
        {
            Debug.Assert(start.Snapshot == end.Snapshot);
            var snapshot = start.Snapshot;

            using (var edit = snapshot.TextBuffer.CreateEdit())
            {
                for (int i = start.GetContainingLine().LineNumber; i <= end.GetContainingLine().LineNumber; i++)
                {
                    var curLine = snapshot.GetLineFromLineNumber(i);

                    DeleteCommentChars(edit, curLine);
                }

                edit.Apply();
            }
        }

        private static void DeleteCommentChars(ITextEdit edit, ITextSnapshotLine curLine)
        {
            var text = curLine.GetText();
            for (int j = 0; j < text.Length; j++)
            {
                if (!char.IsWhiteSpace(text[j]))
                {
                    if (text.Substring(j, 2) == "//")
                    {
                        edit.Delete(curLine.Start.Position + j, 2);
                    }
                    break;
                }
            }
        }

        private static bool IsHlslContent(this ITextSnapshot buffer)
        {
            return buffer.ContentType.IsOfType(HlslConstants.ContentTypeName);
        }

        private static bool IsHlslContent(this ITextBuffer buffer)
        {
            return buffer.ContentType.IsOfType(HlslConstants.ContentTypeName);
        }

        private static void UpdateSelection(ITextView view, SnapshotPoint start, SnapshotPoint end)
        {
            // Select the full region that is commented, do not select if in projection buffer 
            // (the selection might span non-language buffer regions)
            view.Selection.Select(
                new SnapshotSpan(
                    start.GetContainingLine().Start.TranslateTo(view.TextBuffer.CurrentSnapshot, PointTrackingMode.Negative),
                    end.GetContainingLine().End.TranslateTo(view.TextBuffer.CurrentSnapshot, PointTrackingMode.Positive)),
                false);
        }
    }
}