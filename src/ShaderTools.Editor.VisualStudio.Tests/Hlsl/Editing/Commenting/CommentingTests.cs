// Based on https://github.com/Microsoft/nodejstools/blob/1144d653a43b4048ac390bb46a7e255a674c451c/Nodejs/Tests/Core/CommentBlockTests.cs
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

using Microsoft.VisualStudio.Text;
using ShaderTools.Editor.VisualStudio.Hlsl.Editing.Commenting;
using ShaderTools.Editor.VisualStudio.Tests.Hlsl.Support;
using ShaderTools.Editor.VisualStudio.Tests.Support;
using Xunit;

namespace ShaderTools.Editor.VisualStudio.Tests.Hlsl.Editing.Commenting
{
    public class CommentingTests : MefTestsBase
    {
        [WpfFact]
        public void TestCommentCurrentLine()
        {
            var buffer = TextBufferUtility.CreateTextBuffer(Container, @"int i;
float f;");
            var view = TextViewUtility.CreateTextView(Container, buffer);

            view.Caret.MoveTo(view.TextBuffer.CurrentSnapshot.GetLineFromLineNumber(0).Start);

            view.CommentOrUncommentBlock(true);

            Assert.Equal(@"//int i;
float f;",
                view.TextBuffer.CurrentSnapshot.GetText());

            view.Caret.MoveTo(view.TextBuffer.CurrentSnapshot.GetLineFromLineNumber(1).Start);

            view.CommentOrUncommentBlock(true);

            Assert.Equal(@"//int i;
//float f;",
                 view.TextBuffer.CurrentSnapshot.GetText());
        }

        [WpfFact]
        public void TestUnCommentCurrentLine()
        {
            var view = TextViewUtility.CreateTextView(Container,
                TextBufferUtility.CreateTextBuffer(Container, @"//int i;
//float f;"));

            view.Caret.MoveTo(view.TextBuffer.CurrentSnapshot.GetLineFromLineNumber(0).Start);

            view.CommentOrUncommentBlock(false);

            Assert.Equal(@"int i;
//float f;",
                 view.TextBuffer.CurrentSnapshot.GetText());

            view.Caret.MoveTo(view.TextBuffer.CurrentSnapshot.GetLineFromLineNumber(1).Start);

            view.CommentOrUncommentBlock(false);

            Assert.Equal(@"int i;
float f;",
                view.TextBuffer.CurrentSnapshot.GetText());
        }

        [WpfFact]
        public void TestComment()
        {
            var view = TextViewUtility.CreateTextView(Container,
                TextBufferUtility.CreateTextBuffer(Container, @"int i;
float f;"));

            view.Selection.Select(
                new SnapshotSpan(view.TextBuffer.CurrentSnapshot, new Span(0, view.TextBuffer.CurrentSnapshot.Length)),
                false
            );
            view.Selection.IsActive = true;

            view.CommentOrUncommentBlock(true);

            Assert.Equal(@"//int i;
//float f;",
                 view.TextBuffer.CurrentSnapshot.GetText());
        }

        [WpfFact]
        public void TestCommentEmptyLine()
        {
            var view = TextViewUtility.CreateTextView(Container,
                TextBufferUtility.CreateTextBuffer(Container, @"int i;

float f;"));

            view.Selection.Select(
                new SnapshotSpan(view.TextBuffer.CurrentSnapshot, new Span(0, view.TextBuffer.CurrentSnapshot.Length)),
                false
            );
            view.Selection.IsActive = true;

            view.CommentOrUncommentBlock(true);

            Assert.Equal(@"//int i;

//float f;",
                 view.TextBuffer.CurrentSnapshot.GetText());
        }

        [WpfFact]
        public void TestCommentWhiteSpaceLine()
        {
            var view = TextViewUtility.CreateTextView(Container,
                TextBufferUtility.CreateTextBuffer(Container, @"int i;
   
float f;"));

            view.Selection.Select(
                new SnapshotSpan(view.TextBuffer.CurrentSnapshot, new Span(0, view.TextBuffer.CurrentSnapshot.Length)),
                false
            );
            view.Selection.IsActive = true;

            view.CommentOrUncommentBlock(true);

            Assert.Equal(@"//int i;
   
//float f;",
                 view.TextBuffer.CurrentSnapshot.GetText());
        }

        [WpfFact]
        public void TestCommentIndented()
        {
            var view = TextViewUtility.CreateTextView(Container,
                TextBufferUtility.CreateTextBuffer(Container, @"void f(){
    int i;
    half h;
    float f;
}"));

            view.Selection.Select(
                new SnapshotSpan(
                    view.TextBuffer.CurrentSnapshot.GetLineFromLineNumber(1).Start,
                    view.TextBuffer.CurrentSnapshot.GetLineFromLineNumber(2).End
                ),
                false
            );
            view.Selection.IsActive = true;

            view.CommentOrUncommentBlock(true);

            Assert.Equal(@"void f(){
    //int i;
    //half h;
    float f;
}",
                    view.TextBuffer.CurrentSnapshot.GetText());
        }

        [WpfFact]
        public void TestCommentIndentedBlankLine()
        {
            var view = TextViewUtility.CreateTextView(Container,
                TextBufferUtility.CreateTextBuffer(Container, @"void f(){
    int i;

    half h;
    float f;
}"));

            view.Selection.Select(
                new SnapshotSpan(
                    view.TextBuffer.CurrentSnapshot.GetLineFromLineNumber(1).Start,
                    view.TextBuffer.CurrentSnapshot.GetLineFromLineNumber(3).End
                ),
                false
            );
            view.Selection.IsActive = true;

            view.CommentOrUncommentBlock(true);

            Assert.Equal(@"void f(){
    //int i;

    //half h;
    float f;
}",
                    view.TextBuffer.CurrentSnapshot.GetText());
        }

        [WpfFact]
        public void TestCommentBlankLine()
        {
            var view = TextViewUtility.CreateTextView(Container,
                TextBufferUtility.CreateTextBuffer(Container, @"int i;

float f;"));

            view.Caret.MoveTo(view.TextBuffer.CurrentSnapshot.GetLineFromLineNumber(1).Start);

            view.CommentOrUncommentBlock(true);

            Assert.Equal(@"int i;

float f;",
             view.TextBuffer.CurrentSnapshot.GetText());
        }

        [WpfFact]
        public void TestCommentIndentedWhiteSpaceLine()
        {
            var view = TextViewUtility.CreateTextView(Container,
                TextBufferUtility.CreateTextBuffer(Container, @"void f(){
    int i;
  
    half h;
    float f;
}"));

            view.Selection.Select(
                new SnapshotSpan(
                    view.TextBuffer.CurrentSnapshot.GetLineFromLineNumber(1).Start,
                    view.TextBuffer.CurrentSnapshot.GetLineFromLineNumber(3).End
                ),
                false
            );
            view.Selection.IsActive = true;

            view.CommentOrUncommentBlock(true);

            Assert.Equal(@"void f(){
    //int i;
  
    //half h;
    float f;
}",
                    view.TextBuffer.CurrentSnapshot.GetText());
        }

        [WpfFact]
        public void TestUnCommentIndented()
        {
            var view = TextViewUtility.CreateTextView(Container,
                TextBufferUtility.CreateTextBuffer(Container, @"void f(){
    //int i;
    //half h;
    float f;
}"));

            view.Selection.Select(
                new SnapshotSpan(
                    view.TextBuffer.CurrentSnapshot.GetLineFromLineNumber(1).Start,
                    view.TextBuffer.CurrentSnapshot.GetLineFromLineNumber(2).End
                ),
                false
            );
            view.Selection.IsActive = true;

            view.CommentOrUncommentBlock(false);

            Assert.Equal(@"void f(){
    int i;
    half h;
    float f;
}",
                    view.TextBuffer.CurrentSnapshot.GetText());
        }

        [WpfFact]
        public void TestUnComment()
        {
            var view = TextViewUtility.CreateTextView(Container,
                TextBufferUtility.CreateTextBuffer(Container, @"//int i;
//float f;"));

            view.Selection.Select(
                new SnapshotSpan(view.TextBuffer.CurrentSnapshot, new Span(0, view.TextBuffer.CurrentSnapshot.Length)),
                false
            );
            view.Selection.IsActive = true;

            view.CommentOrUncommentBlock(false);

            Assert.Equal(@"int i;
float f;",
                view.TextBuffer.CurrentSnapshot.GetText());
        }

        [WpfFact]
        public void TestCommentStartOfLastLine()
        {
            var view = TextViewUtility.CreateTextView(Container,
                TextBufferUtility.CreateTextBuffer(Container, @"int i;
float f;"));

            view.Selection.Select(
                new SnapshotSpan(view.TextBuffer.CurrentSnapshot, new Span(0, view.TextBuffer.CurrentSnapshot.GetText().IndexOf("float f;"))),
                false
            );
            view.Selection.IsActive = true;

            view.CommentOrUncommentBlock(true);

            Assert.Equal(@"//int i;
float f;",
                view.TextBuffer.CurrentSnapshot.GetText());
        }

        [WpfFact]
        public void TestCommentAfterCodeIsNotUncommented()
        {
            var view = TextViewUtility.CreateTextView(Container,
                TextBufferUtility.CreateTextBuffer(Container, @"int i;//comment that should stay a comment;
//half h;//another comment that should stay a comment;
float f;"));

            view.Selection.Select(
                new SnapshotSpan(view.TextBuffer.CurrentSnapshot, new Span(0, view.TextBuffer.CurrentSnapshot.GetText().IndexOf("float f;"))),
                false
            );
            view.Selection.IsActive = true;

            view.CommentOrUncommentBlock(false);

            Assert.Equal(@"int i;//comment that should stay a comment;
half h;//another comment that should stay a comment;
float f;",
                view.TextBuffer.CurrentSnapshot.GetText());
        }
    }
}