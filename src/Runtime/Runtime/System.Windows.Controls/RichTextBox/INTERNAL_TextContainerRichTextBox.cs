﻿#if MIGRATION
using System.Windows.Documents;

namespace System.Windows.Controls
#else
using Windows.UI.Xaml.Documents;

namespace Windows.UI.Xaml.Controls
#endif
{
    internal class INTERNAL_TextContainerRichTextBox : INTERNAL_TextContainer
    {
        private RichTextBox _parent;
        public INTERNAL_TextContainerRichTextBox(RichTextBox parent)
            :base(parent)
        {
            _parent = parent;
        }

        public override string Text => _parent.GetRawText();

        protected override void OnTextAddedOverride(TextElement textElement)
        {
            if(textElement is Paragraph paragraph)
            {
                string text = "";
                foreach(var inline in paragraph.Inlines)
                {
                    if(inline is Run run)
                    {
                        text += run.Text;
                    }
                    //TODO: support other Inlines
                }

                _parent.InsertText(text);
            }
            else if(textElement is Section)
            {
                //Does not support now
            }
        }

        protected override void OnTextRemovedOverride(TextElement textElement)
        {
            //TODO: implement
        }
    }
}