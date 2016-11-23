// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileHeader.cs" company="https://github.com/StyleCop">
//   MS-PL
// </copyright>
// <license>
//   This source code is subject to terms and conditions of the Microsoft 
//   Public License. A copy of the license can be found in the License.html 
//   file at the root of this distribution. If you cannot locate the  
//   Microsoft Public License, please send an email to dlr@microsoft.com. 
//   By using this source code in any fashion, you are agreeing to be bound 
//   by the terms of the Microsoft Public License. You must not remove this 
//   notice, or any other, from this software.
// </license>
// <summary>
//   Describes the header at the top of a C# file.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace StyleCop.CSharp
{
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Linq;
    using System.Xml;

    /// <summary>
    ///   Describes the header at the top of a C# file.
    /// </summary>
    /// <subcategory>other</subcategory>
    public class FileHeader : ICodePart
    {
        #region Fields

        /// <summary>
        ///   Indicates whether the file header has the generated attribute.
        /// </summary>
        private readonly bool generated;

        /// <summary>
        ///   The header text.
        /// </summary>
        private readonly string headerText;

        /// <summary>
        ///   The header text wrapped into an Xml tag.
        /// </summary>
        private readonly string headerXml;

        /// <summary>
        ///   The location of the header.
        /// </summary>
        private readonly CodeLocation location;

        /// <summary>
        ///   The parent of the file header.
        /// </summary>
        private readonly Reference<ICodePart> parent;

        /// <summary>
        ///   The collection of tokens in the header.
        /// </summary>
        private readonly CsTokenList tokens;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the FileHeader class.
        /// </summary>
        /// <param name="headerText">
        /// The header text. 
        /// </param>
        /// <param name="tokens">
        /// The collection of tokens in the header. 
        /// </param>
        /// <param name="parent">
        /// The parent of the header. 
        /// </param>
        internal FileHeader(string headerText, CsTokenList tokens, Reference<ICodePart> parent)
        {
            Param.AssertNotNull(headerText, "headerText");
            Param.AssertNotNull(tokens, "tokens");
            Param.AssertNotNull(parent, "parent");

            this.headerText = headerText;
            this.tokens = tokens;
            this.parent = parent;

            this.location = this.tokens.First != null ? CsToken.JoinLocations(this.tokens.First, this.tokens.Last) : CodeLocation.Empty;

            // Attempt to load this into an Xml document.
            try
            {
                if (this.headerText.Length > 0)
                {
                    this.headerXml = string.Format(CultureInfo.InvariantCulture, "<root>{0}</root>", HtmlEncode(this.headerText));

                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(this.headerXml);

                    // Check whether the header has the autogenerated tag.
                    if (doc.DocumentElement != null)
                    {
                        XmlNode node = doc.DocumentElement["autogenerated"];
                        if (node != null)
                        {
                            // Set this as generated code.
                            this.generated = true;
                        }
                        else
                        {
                            node = doc.DocumentElement["auto-generated"];
                            if (node != null)
                            {
                                // Set this as generated code.
                                this.generated = true;
                            }
                        }

                        StringCollection unstyledElements = new StringCollection();
                        unstyledElements.AddRange(new[] { "unstyled", "stylecopoff", "nostyle" });

                        XmlNodeList childNodes = doc.DocumentElement.ChildNodes;
                        if (childNodes.Cast<XmlNode>().Any(xmlNode => unstyledElements.Contains(xmlNode.Name.ToLowerInvariant())))
                        {
                            this.UnStyled = true;
                        }
                    }
                }
            }
            catch (XmlException)
            {
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///   Gets the type of this code part.
        /// </summary>
        public CodePartType CodePartType
        {
            get
            {
                return CodePartType.FileHeader;
            }
        }

        /// <summary>
        ///   Gets a value indicating whether the file header contains the auto-generated attribute.
        /// </summary>
        public bool Generated
        {
            get
            {
                return this.generated;
            }
        }

        /// <summary>
        ///   Gets the header text.
        /// </summary>
        public string HeaderText
        {
            get
            {
                return this.headerText;
            }
        }

        /// <summary>
        ///   Gets the header text string modified such that it is loadable into an <see cref="XmlDocument" /> object.
        /// </summary>
        public string HeaderXml
        {
            get
            {
                return this.headerXml;
            }
        }

        /// <summary>
        ///   Gets the line number on which the header begins.
        /// </summary>
        public int LineNumber
        {
            get
            {
                return this.location.LineNumber;
            }
        }

        /// <summary>
        ///   Gets the location of the token.
        /// </summary>
        public CodeLocation Location
        {
            get
            {
                return this.location;
            }
        }

        /// <summary>
        ///   Gets the parent of the file header.
        /// </summary>
        public ICodePart Parent
        {
            get
            {
                return this.parent.Target;
            }
        }

        /// <summary>
        ///   Gets the collection of tokens that form the header.
        /// </summary>
        public CsTokenList Tokens
        {
            get
            {
                return this.tokens;
            }
        }

        /// <summary>
        ///   Gets a value indicating whether the header has a UnStyled element.
        /// </summary>
        public bool UnStyled { get; private set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Converts a string to an HTML-encoded string.
        /// </summary>
        /// <returns>
        /// An encoded string. 
        /// </returns>
        /// <param name="value">
        /// The string to encode. 
        /// </param>
        public static string HtmlEncode(string value)
        {
            return value.Replace("&", "&amp;");
        }

        #endregion
    }
}