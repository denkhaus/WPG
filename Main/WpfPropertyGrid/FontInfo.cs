using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;

namespace System.Windows.Controls.WpfPropertyGrid
{
	public static class TypefaceExtensions
	{
		/// <summary>
		/// Gets the typeface name.
		/// </summary>
		/// <param name="typeface">The typeface.</param>
		/// <returns></returns>
		/// <remarks>
		/// Gets the typeface name for either the current Culture, en-us culture or the
		/// first FaceName available, in that order.
		/// <para>Note: When new APIs are available to obtain the language-specific face name
		/// from the font this code will need to be replaced with calls to those new APIs.</para>
		/// </remarks>
		public static string Name(this Typeface typeface)
		{
			if (typeface == null)
				return null;
			IDictionary<XmlLanguage, string> faceNames = typeface.FaceNames;
			if (faceNames.Count == 0)
				return null;
			string faceName;
			if (!faceNames.TryGetValue(XmlLanguage.GetLanguage(CultureInfo.CurrentUICulture.IetfLanguageTag), out faceName)
				&& !faceNames.TryGetValue(XmlLanguage.GetLanguage("en-us"), out faceName))
			{ // The typeface doesn't have a FaceName neither for the CurrentUICulture 
				// nor for the "en-us" culture.
				// Get the first FaceName available.
				foreach (KeyValuePair<XmlLanguage, string> pair in faceNames)
				{
					faceName = pair.Value;
					break;
				}
			}
			return faceName;
		}
	}


	public class FontInfo : ICloneable, INotifyPropertyChanged
	{
		private static readonly CultureInfo defaultTextCulture = new CultureInfo("en-US");

		private static readonly double[]	standardFontSizes				= new[]
																				{
																					3.0d, 4.0d, 5.0d, 6.0d, 6.5d,
																					7.0d, 7.5d, 8.0d, 8.5d, 9.0d,
																					9.5d, 10.0d, 10.5d, 11.0d, 11.5d,
																					12.0d, 12.5d, 13.0d, 13.5d, 14.0d,
																					15.0d, 16.0d, 17.0d, 18.0d, 19.0d,
																					20.0d, 22.0d, 24.0d, 26.0d, 28.0d, 30.0d, 32.0d, 34.0d, 36.0d, 38.0d,
																					40.0d, 44.0d, 48.0d, 52.0d, 56.0d, 60.0d, 64.0d, 68.0d, 72.0d, 76.0d,
																					80.0d, 88.0d, 96.0d, 104.0d, 112.0d, 120.0d, 128.0d, 136.0d, 144.0d, 152.0d,
																					160.0d, 176.0d, 192.0d, 208.0d, 224.0d, 240.0d, 256.0d, 272.0d, 288.0d, 304.0d,
																					320.0d, 352.0d, 384.0d, 416.0d, 448.0d, 480.0d, 512.0d, 544.0d, 576.0d, 608.0d,
																					640.0d
																				};

		private int							annotationAlternates;
		private bool						baseline;
		private bool						capitalSpacing;
		private FontCapitals				capitals						= FontCapitals.Normal;
		private bool						caseSensitiveForms;
		private bool						contextualAlternates;
		private bool						contextualLigatures;
		private int							contextualSwashes;
		private CultureInfo					descriptiveTextCulture			= defaultTextCulture;
		private IEnumerable<CultureInfo>	descriptiveTextCultures;
		private bool						discretionaryLigatures;
		private bool						eastAsianExpertForms;
		private FontEastAsianLanguage		eastAsianLanguage				= FontEastAsianLanguage.Normal;
		private FontEastAsianWidths			eastAsianWidths					= FontEastAsianWidths.Normal;
		private FontFamily					fontFamily						= new FontFamily("Lucida Console, Tahoma, Microsoft Sans Serif, Times New Roman");
		private double						fontSize						= 8.0;
		private FontFraction				fraction						= FontFraction.Normal;
		private bool						historicalForms;
		private bool						historicalLigatures;
		private bool						kerning;
		private bool						mathematicalGreek;
		private FontNumeralAlignment		numeralAlignment				= FontNumeralAlignment.Normal;
		private FontNumeralStyle			numeralStyle					= FontNumeralStyle.Normal;
		private bool						overline;
		private bool						slashedZero;
		private bool						standardLigatures;
		private int							standardSwashes;
		private bool						strikethrough;
		private int							stylisticAlternates;
		private bool						stylisticSet1;
		private bool						stylisticSet10;
		private bool						stylisticSet11;
		private bool						stylisticSet12;
		private bool						stylisticSet13;
		private bool						stylisticSet14;
		private bool						stylisticSet15;
		private bool						stylisticSet16;
		private bool						stylisticSet17;
		private bool						stylisticSet18;
		private bool						stylisticSet19;
		private bool						stylisticSet2;
		private bool						stylisticSet20;
		private bool						stylisticSet3;
		private bool						stylisticSet4;
		private bool						stylisticSet5;
		private bool						stylisticSet6;
		private bool						stylisticSet7;
		private bool						stylisticSet8;
		private bool						stylisticSet9;
		private Typeface					typeface;
		private bool						underline;
		private FontVariants				variants						= FontVariants.Normal;

		public int AnnotationAlternates
		{
			get { return annotationAlternates; }
			set
			{
				if (annotationAlternates == value) return;
				annotationAlternates = value;
				NotifyPropertyChanged("AnnotationAlternates");
			}
		}

		public void ApplyTo(DependencyObject dependencyObject)
		{
			dependencyObject.SetValue(TextElement.FontFamilyProperty, FontFamily);
			dependencyObject.SetValue(TextElement.FontStyleProperty, Typeface.Style);
			dependencyObject.SetValue(TextElement.FontWeightProperty, Typeface.Weight);
			dependencyObject.SetValue(TextElement.FontStretchProperty, Typeface.Stretch);
			dependencyObject.SetValue(TextElement.FontSizeProperty, FontSize);
			dependencyObject.SetValue(Inline.TextDecorationsProperty, Decorations);
			// Typography
			Typography.SetAnnotationAlternates(dependencyObject, AnnotationAlternates);
			Typography.SetCapitals(dependencyObject, Capitals);
			Typography.SetCapitalSpacing(dependencyObject, CapitalSpacing);
			Typography.SetCaseSensitiveForms(dependencyObject, CaseSensitiveForms);
			Typography.SetContextualAlternates(dependencyObject, ContextualAlternates);
			Typography.SetContextualLigatures(dependencyObject, ContextualLigatures);
			Typography.SetContextualSwashes(dependencyObject, ContextualSwashes);
			Typography.SetDiscretionaryLigatures(dependencyObject, DiscretionaryLigatures);
			Typography.SetEastAsianExpertForms(dependencyObject, EastAsianExpertForms);
			Typography.SetEastAsianLanguage(dependencyObject, EastAsianLanguage);
			Typography.SetEastAsianWidths(dependencyObject, EastAsianWidths);
			Typography.SetFraction(dependencyObject, Fraction);
			Typography.SetHistoricalForms(dependencyObject, HistoricalForms);
			Typography.SetHistoricalLigatures(dependencyObject, HistoricalLigatures);
			Typography.SetKerning(dependencyObject, Kerning);
			Typography.SetMathematicalGreek(dependencyObject, MathematicalGreek);
			Typography.SetNumeralAlignment(dependencyObject, NumeralAlignment);
			Typography.SetNumeralStyle(dependencyObject, NumeralStyle);
			Typography.SetSlashedZero(dependencyObject, SlashedZero);
			Typography.SetStandardLigatures(dependencyObject, StandardLigatures);
			Typography.SetStandardSwashes(dependencyObject, StandardSwashes);
			Typography.SetStylisticAlternates(dependencyObject, StylisticAlternates);
			Typography.SetStylisticSet1(dependencyObject, StylisticSet1);
			Typography.SetStylisticSet2(dependencyObject, StylisticSet2);
			Typography.SetStylisticSet3(dependencyObject, StylisticSet3);
			Typography.SetStylisticSet4(dependencyObject, StylisticSet4);
			Typography.SetStylisticSet5(dependencyObject, StylisticSet5);
			Typography.SetStylisticSet6(dependencyObject, StylisticSet6);
			Typography.SetStylisticSet7(dependencyObject, StylisticSet7);
			Typography.SetStylisticSet8(dependencyObject, StylisticSet8);
			Typography.SetStylisticSet9(dependencyObject, StylisticSet9);
			Typography.SetStylisticSet10(dependencyObject, StylisticSet10);
			Typography.SetStylisticSet11(dependencyObject, StylisticSet11);
			Typography.SetStylisticSet12(dependencyObject, StylisticSet12);
			Typography.SetStylisticSet13(dependencyObject, StylisticSet13);
			Typography.SetStylisticSet14(dependencyObject, StylisticSet14);
			Typography.SetStylisticSet15(dependencyObject, StylisticSet15);
			Typography.SetStylisticSet16(dependencyObject, StylisticSet16);
			Typography.SetStylisticSet17(dependencyObject, StylisticSet17);
			Typography.SetStylisticSet18(dependencyObject, StylisticSet18);
			Typography.SetStylisticSet19(dependencyObject, StylisticSet19);
			Typography.SetStylisticSet20(dependencyObject, StylisticSet20);
			Typography.SetVariants(dependencyObject, Variants);
		}

		public bool Baseline
		{
			get { return baseline; }
			set
			{
				if (baseline == value) return;
				baseline = value;
				NotifyPropertyChanged("Baseline");
				NotifyPropertyChanged("Decorations");
			}
		}

		public FontCapitals Capitals
		{
			get { return capitals; }
			set
			{
				if (capitals == value) return;
				capitals = value;
				NotifyPropertyChanged("Capitals");
			}
		}

		public bool CapitalSpacing
		{
			get { return capitalSpacing; }
			set
			{
				if (capitalSpacing == value) return;
				capitalSpacing = value;
				NotifyPropertyChanged("CapitalSpacing");
			}
		}

		public bool CaseSensitiveForms
		{
			get { return caseSensitiveForms; }
			set
			{
				if (caseSensitiveForms == value) return;
				caseSensitiveForms = value;
				NotifyPropertyChanged("CaseSensitiveForms");
			}
		}

		public bool ContextualAlternates
		{
			get { return contextualAlternates; }
			set
			{
				if (contextualAlternates == value) return;
				contextualAlternates = value;
				NotifyPropertyChanged("ContextualAlternates");
			}
		}

		public bool ContextualLigatures
		{
			get { return contextualLigatures; }
			set
			{
				if (contextualLigatures == value) return;
				contextualLigatures = value;
				NotifyPropertyChanged("ContextualLigatures");
			}
		}

		public int ContextualSwashes
		{
			get { return contextualSwashes; }
			set
			{
				if (contextualSwashes == value) return;
				contextualSwashes = value;
				NotifyPropertyChanged("ContextualSwashes");
			}
		}

		public TextDecorationCollection Decorations
		{
			get
			{
				TextDecorationCollection decorations = new TextDecorationCollection();
				if (Strikethrough) decorations.Add(TextDecorations.Strikethrough[0]);
				if (Underline) decorations.Add(TextDecorations.Underline[0]);
				if (Overline) decorations.Add(TextDecorations.OverLine[0]);
				if (Baseline) decorations.Add(TextDecorations.Baseline[0]);
				return decorations;
			}
			set
			{
				foreach (TextDecoration decoration in value)
					if (decoration.Equals(TextDecorations.Strikethrough[0])) Strikethrough = true;
					else if (decoration.Equals(TextDecorations.Underline[0])) Underline = true;
					else if (decoration.Equals(TextDecorations.OverLine[0])) Overline = true;
					else if (decoration.Equals(TextDecorations.Baseline[0])) Baseline = true;
			}
		}

		public CultureInfo DescriptiveTextCulture
		{
			get { return descriptiveTextCulture; }
			set
			{
				if (descriptiveTextCulture == value) return;
				descriptiveTextCulture = value;
				NotifyPropertyChanged("DescriptiveTextCulture");

				NotifyPropertyChanged("GlyphTypeface");
				NotifyPropertyChanged("GlyphTypefaceFontUri");
				NotifyPropertyChanged("GlyphTypefaceCopyright");
				NotifyPropertyChanged("GlyphTypefaceDescription");
				NotifyPropertyChanged("GlyphTypefaceDesignerName");
				NotifyPropertyChanged("GlyphTypefaceDesignerUrl");
				NotifyPropertyChanged("GlyphTypefaceFaceName");
				NotifyPropertyChanged("GlyphTypefaceFamilyName");
				NotifyPropertyChanged("GlyphTypefaceManufacturerName");
				NotifyPropertyChanged("GlyphTypefaceSampleText");
				NotifyPropertyChanged("GlyphTypefaceTrademark");
				NotifyPropertyChanged("GlyphTypefaceVendorUrl");
				NotifyPropertyChanged("GlyphTypefaceVersionString");
				NotifyPropertyChanged("GlyphTypefaceWin32FaceName");
				NotifyPropertyChanged("GlyphTypefaceWin32FamilyName");
				NotifyPropertyChanged("GlyphTypefaceLicenseDescription");
			}
		}

		public IEnumerable<CultureInfo> DescriptiveTextCultures
		{
			get
			{
				if (descriptiveTextCultures == null)
				{
					descriptiveTextCultures = from c in CultureInfo.GetCultures(CultureTypes.AllCultures)
											  where GetGlyphTypefaceCopyright(c) != null
													| GetGlyphTypefaceDescription(c) != null
													| GetGlyphTypefaceDesignerName(c) != null
													| GetGlyphTypefaceDesignerUrl(c) != null
													| GetGlyphTypefaceFaceName(c) != null
													| GetGlyphTypefaceFamilyName(c) != null
													| GetGlyphTypefaceManufacturerName(c) != null
													| GetGlyphTypefaceSampleText(c) != null
													| GetGlyphTypefaceTrademark(c) != null
													| GetGlyphTypefaceVendorUrl(c) != null
													| GetGlyphTypefaceVersionString(c) != null
													| GetGlyphTypefaceWin32FaceName(c) != null
													| GetGlyphTypefaceWin32FamilyName(c) != null
											  select c;

					if (!descriptiveTextCultures.Contains(DescriptiveTextCulture))
						DescriptiveTextCulture = defaultTextCulture;
					else
					{
						NotifyPropertyChanged("GlyphTypeface");
						NotifyPropertyChanged("GlyphTypefaceFontUri");
						NotifyPropertyChanged("GlyphTypefaceCopyright");
						NotifyPropertyChanged("GlyphTypefaceDescription");
						NotifyPropertyChanged("GlyphTypefaceDesignerName");
						NotifyPropertyChanged("GlyphTypefaceDesignerUrl");
						NotifyPropertyChanged("GlyphTypefaceFaceName");
						NotifyPropertyChanged("GlyphTypefaceFamilyName");
						NotifyPropertyChanged("GlyphTypefaceManufacturerName");
						NotifyPropertyChanged("GlyphTypefaceSampleText");
						NotifyPropertyChanged("GlyphTypefaceTrademark");
						NotifyPropertyChanged("GlyphTypefaceVendorUrl");
						NotifyPropertyChanged("GlyphTypefaceVersionString");
						NotifyPropertyChanged("GlyphTypefaceWin32FaceName");
						NotifyPropertyChanged("GlyphTypefaceWin32FamilyName");
						NotifyPropertyChanged("GlyphTypefaceLicenseDescription");
					}
				}
				return descriptiveTextCultures;
			}
		}

		public bool DiscretionaryLigatures
		{
			get { return discretionaryLigatures; }
			set
			{
				if (discretionaryLigatures == value) return;
				discretionaryLigatures = value;
				NotifyPropertyChanged("DiscretionaryLigatures");
			}
		}

		public string DisplayName
		{
			get { return string.Format("{0}, {1}, {2}", FontFamily.Source, FontSize, Typeface.Name()); }
		}

		public bool EastAsianExpertForms
		{
			get { return eastAsianExpertForms; }
			set
			{
				if (eastAsianExpertForms == value) return;
				eastAsianExpertForms = value;
				NotifyPropertyChanged("EastAsianExpertForms");
			}
		}

		public FontEastAsianLanguage EastAsianLanguage
		{
			get { return eastAsianLanguage; }
			set
			{
				if (eastAsianLanguage == value) return;
				eastAsianLanguage = value;
				NotifyPropertyChanged("EastAsianLanguage");
			}
		}

		public FontEastAsianWidths EastAsianWidths
		{
			get { return eastAsianWidths; }
			set
			{
				if (eastAsianWidths == value) return;
				eastAsianWidths = value;
				NotifyPropertyChanged("EastAsianWidths");
			}
		}

		public static IEnumerable<FontFamily> FontFamilies
		{
			get { return Fonts.SystemFontFamilies.OrderBy(family => family.Source); }
		}

		public FontFamily FontFamily
		{
			get { return fontFamily; }
			set
			{
				if (fontFamily == value) return;
				Typeface newTypeface = SelectBestMatchingTypeface(value);

				fontFamily = value;
				NotifyPropertyChanged("FontFamily");
				NotifyPropertyChanged("Typefaces");
				NotifyPropertyChanged("NamedTypefaces");
				NotifyPropertyChanged("DisplayName");
				Typeface = newTypeface;
			}
		}

		public FontInfo()
		{
		}

		public FontInfo(DependencyObject dependencyObject)
		{
			FontFamily	= dependencyObject.GetValue(TextElement.FontFamilyProperty) as FontFamily;
			Typeface	= SelectBestMatchingTypeface(FontFamily
												  , (FontStyle)dependencyObject.GetValue(TextElement.FontStyleProperty)
												  , (FontWeight)dependencyObject.GetValue(TextElement.FontWeightProperty)
												  , (FontStretch)dependencyObject.GetValue(TextElement.FontStretchProperty));
			FontSize	= (double)dependencyObject.GetValue(TextElement.FontSizeProperty);
			Decorations	= dependencyObject.GetValue(Inline.TextDecorationsProperty) as TextDecorationCollection;

			AnnotationAlternates	= Typography.GetAnnotationAlternates(dependencyObject);
			Capitals				= Typography.GetCapitals(dependencyObject);
			CapitalSpacing			= Typography.GetCapitalSpacing(dependencyObject);
			CaseSensitiveForms		= Typography.GetCaseSensitiveForms(dependencyObject);
			ContextualAlternates	= Typography.GetContextualAlternates(dependencyObject);
			ContextualLigatures		= Typography.GetContextualLigatures(dependencyObject);
			ContextualSwashes		= Typography.GetContextualSwashes(dependencyObject);
			DiscretionaryLigatures	= Typography.GetDiscretionaryLigatures(dependencyObject);
			EastAsianExpertForms	= Typography.GetEastAsianExpertForms(dependencyObject);
			EastAsianLanguage		= Typography.GetEastAsianLanguage(dependencyObject);
			EastAsianWidths			= Typography.GetEastAsianWidths(dependencyObject);
			Fraction				= Typography.GetFraction(dependencyObject);
			HistoricalForms			= Typography.GetHistoricalForms(dependencyObject);
			HistoricalLigatures		= Typography.GetHistoricalLigatures(dependencyObject);
			Kerning					= Typography.GetKerning(dependencyObject);
			MathematicalGreek		= Typography.GetMathematicalGreek(dependencyObject);
			NumeralAlignment		= Typography.GetNumeralAlignment(dependencyObject);
			NumeralStyle			= Typography.GetNumeralStyle(dependencyObject);
			SlashedZero				= Typography.GetSlashedZero(dependencyObject);
			StandardLigatures		= Typography.GetStandardLigatures(dependencyObject);
			StandardSwashes			= Typography.GetStandardSwashes(dependencyObject);
			StylisticAlternates		= Typography.GetStylisticAlternates(dependencyObject);
			StylisticSet1			= Typography.GetStylisticSet1(dependencyObject);
			StylisticSet2			= Typography.GetStylisticSet2(dependencyObject);
			StylisticSet3			= Typography.GetStylisticSet3(dependencyObject);
			StylisticSet4			= Typography.GetStylisticSet4(dependencyObject);
			StylisticSet5			= Typography.GetStylisticSet5(dependencyObject);
			StylisticSet6			= Typography.GetStylisticSet6(dependencyObject);
			StylisticSet7			= Typography.GetStylisticSet7(dependencyObject);
			StylisticSet8			= Typography.GetStylisticSet8(dependencyObject);
			StylisticSet9			= Typography.GetStylisticSet9(dependencyObject);
			StylisticSet10			= Typography.GetStylisticSet10(dependencyObject);
			StylisticSet11			= Typography.GetStylisticSet11(dependencyObject);
			StylisticSet12			= Typography.GetStylisticSet12(dependencyObject);
			StylisticSet13			= Typography.GetStylisticSet13(dependencyObject);
			StylisticSet14			= Typography.GetStylisticSet14(dependencyObject);
			StylisticSet15			= Typography.GetStylisticSet15(dependencyObject);
			StylisticSet16			= Typography.GetStylisticSet16(dependencyObject);
			StylisticSet17			= Typography.GetStylisticSet17(dependencyObject);
			StylisticSet18			= Typography.GetStylisticSet18(dependencyObject);
			StylisticSet19			= Typography.GetStylisticSet19(dependencyObject);
			StylisticSet20			= Typography.GetStylisticSet20(dependencyObject);
			Variants				= Typography.GetVariants(dependencyObject);
		}

		public double FontSize
		{
			get { return fontSize; }
			set
			{
				if (fontSize == value) return;
				fontSize = value;
				NotifyPropertyChanged("FontSize");
				NotifyPropertyChanged("SampleFontSizes");
				NotifyPropertyChanged("DisplayName");
			}
		}

		public double[] FontSizes
		{
			get { return standardFontSizes; }
		}

		public FontFraction Fraction
		{
			get { return fraction; }
			set
			{
				if (fraction == value) return;
				fraction = value;
				NotifyPropertyChanged("Fraction");
			}
		}

		private string GetGlyphTypefaceCopyright(CultureInfo culture)
		{
			GlyphTypeface glyphTypeface = GlyphTypeface;
			if (glyphTypeface == null)
				return null;
			string str;
			glyphTypeface.Copyrights.TryGetValue(culture, out str);
			return str;
		}

		private string GetGlyphTypefaceDescription(CultureInfo culture)
		{
			GlyphTypeface glyphTypeface = GlyphTypeface;
			if (glyphTypeface == null)
				return null;
			string str;
			glyphTypeface.Descriptions.TryGetValue(culture, out str);
			return str;
		}

		private string GetGlyphTypefaceDesignerName(CultureInfo culture)
		{
			GlyphTypeface glyphTypeface = GlyphTypeface;
			if (glyphTypeface == null)
				return null;
			string str;
			glyphTypeface.DesignerNames.TryGetValue(culture, out str);
			return str;
		}

		private string GetGlyphTypefaceDesignerUrl(CultureInfo culture)
		{
			GlyphTypeface glyphTypeface = GlyphTypeface;
			if (glyphTypeface == null)
				return null;
			string str;
			glyphTypeface.DesignerUrls.TryGetValue(culture, out str);
			return str;
		}

		private string GetGlyphTypefaceFaceName(CultureInfo culture)
		{
			GlyphTypeface glyphTypeface = GlyphTypeface;
			if (glyphTypeface == null)
				return null;
			string str;
			glyphTypeface.FaceNames.TryGetValue(culture, out str);
			return str;
		}

		private string GetGlyphTypefaceFamilyName(CultureInfo culture)
		{
			GlyphTypeface glyphTypeface = GlyphTypeface;
			if (glyphTypeface == null)
				return null;
			string str;
			glyphTypeface.FamilyNames.TryGetValue(culture, out str);
			return str;
		}

		private string GetGlyphTypefaceLicenseDescription(CultureInfo culture)
		{
			GlyphTypeface glyphTypeface = GlyphTypeface;
			if (glyphTypeface == null)
				return null;
			string str;
			glyphTypeface.LicenseDescriptions.TryGetValue(culture, out str);
			return str;
		}

		private string GetGlyphTypefaceManufacturerName(CultureInfo culture)
		{
			GlyphTypeface glyphTypeface = GlyphTypeface;
			if (glyphTypeface == null)
				return null;
			string str;
			glyphTypeface.ManufacturerNames.TryGetValue(culture, out str);
			return str;
		}

		private string GetGlyphTypefaceSampleText(CultureInfo culture)
		{
			GlyphTypeface glyphTypeface = GlyphTypeface;
			if (glyphTypeface == null)
				return null;
			string str;
			glyphTypeface.SampleTexts.TryGetValue(culture, out str);
			return str;
		}

		private string GetGlyphTypefaceTrademark(CultureInfo culture)
		{
			GlyphTypeface glyphTypeface = GlyphTypeface;
			if (glyphTypeface == null)
				return null;
			string str;
			glyphTypeface.Trademarks.TryGetValue(culture, out str);
			return str;
		}

		private string GetGlyphTypefaceVendorUrl(CultureInfo culture)
		{
			GlyphTypeface glyphTypeface = GlyphTypeface;
			if (glyphTypeface == null)
				return null;
			string str;
			glyphTypeface.VendorUrls.TryGetValue(culture, out str);
			return str;
		}

		private string GetGlyphTypefaceVersionString(CultureInfo culture)
		{
			GlyphTypeface glyphTypeface = GlyphTypeface;
			if (glyphTypeface == null)
				return null;
			string str;
			glyphTypeface.VersionStrings.TryGetValue(culture, out str);
			return str;
		}

		private string GetGlyphTypefaceWin32FaceName(CultureInfo culture)
		{
			GlyphTypeface glyphTypeface = GlyphTypeface;
			if (glyphTypeface == null)
				return null;
			string str;
			glyphTypeface.Win32FaceNames.TryGetValue(culture, out str);
			return str;
		}

		private string GetGlyphTypefaceWin32FamilyName(CultureInfo culture)
		{
			GlyphTypeface glyphTypeface = GlyphTypeface;
			if (glyphTypeface == null)
				return null;
			string str;
			glyphTypeface.Win32FamilyNames.TryGetValue(culture, out str);
			return str;
		}

		public GlyphTypeface GlyphTypeface
		{
			get
			{
				GlyphTypeface glyphTypeface = null;
				if (Typeface != null)
					Typeface.TryGetGlyphTypeface(out glyphTypeface);
				return glyphTypeface;
			}
		}

		public string GlyphTypefaceCopyright
		{
			get { return GetGlyphTypefaceCopyright(DescriptiveTextCulture); }
		}

		public string GlyphTypefaceDescription
		{
			get { return GetGlyphTypefaceDescription(DescriptiveTextCulture); }
		}

		public string GlyphTypefaceDesignerName
		{
			get { return GetGlyphTypefaceDesignerName(DescriptiveTextCulture); }
		}

		public string GlyphTypefaceDesignerUrl
		{
			get { return GetGlyphTypefaceDesignerUrl(DescriptiveTextCulture); }
		}

		public string GlyphTypefaceFaceName
		{
			get { return GetGlyphTypefaceFaceName(DescriptiveTextCulture); }
		}

		public string GlyphTypefaceFamilyName
		{
			get { return GetGlyphTypefaceFamilyName(DescriptiveTextCulture); }
		}

		public Uri GlyphTypefaceFontUri
		{
			get { return GlyphTypeface == null ? null : GlyphTypeface.FontUri; }
		}

		public string GlyphTypefaceLicenseDescription
		{
			get { return GetGlyphTypefaceLicenseDescription(DescriptiveTextCulture); }
		}

		public string GlyphTypefaceManufacturerName
		{
			get { return GetGlyphTypefaceManufacturerName(DescriptiveTextCulture); }
		}

		public string GlyphTypefaceSampleText
		{
			get { return GetGlyphTypefaceSampleText(DescriptiveTextCulture); }
		}

		public string GlyphTypefaceTrademark
		{
			get { return GetGlyphTypefaceTrademark(DescriptiveTextCulture); }
		}

		public string GlyphTypefaceVendorUrl
		{
			get { return GetGlyphTypefaceVendorUrl(DescriptiveTextCulture); }
		}

		public string GlyphTypefaceVersionString
		{
			get { return GetGlyphTypefaceVersionString(DescriptiveTextCulture); }
		}

		public string GlyphTypefaceWin32FaceName
		{
			get { return GetGlyphTypefaceWin32FaceName(DescriptiveTextCulture); }
		}

		public string GlyphTypefaceWin32FamilyName
		{
			get { return GetGlyphTypefaceWin32FamilyName(DescriptiveTextCulture); }
		}

		public bool HistoricalForms
		{
			get { return historicalForms; }
			set
			{
				if (historicalForms == value) return;
				historicalForms = value;
				NotifyPropertyChanged("HistoricalForms");
			}
		}

		public bool HistoricalLigatures
		{
			get { return historicalLigatures; }
			set
			{
				if (historicalLigatures == value) return;
				historicalLigatures = value;
				NotifyPropertyChanged("HistoricalLigatures");
			}
		}

		public bool Kerning
		{
			get { return kerning; }
			set
			{
				if (kerning == value) return;
				kerning = value;
				NotifyPropertyChanged("Kerning");
			}
		}

		public bool MathematicalGreek
		{
			get { return mathematicalGreek; }
			set
			{
				if (mathematicalGreek == value) return;
				mathematicalGreek = value;
				NotifyPropertyChanged("MathematicalGreek");
			}
		}

		public IEnumerable<NamedTypeface> NamedTypefaces
		{
			get
			{
				return from tf in Typefaces
					   select new NamedTypeface(tf);
			}
		}

		public FontNumeralAlignment NumeralAlignment
		{
			get { return numeralAlignment; }
			set
			{
				if (numeralAlignment == value) return;
				numeralAlignment = value;
				NotifyPropertyChanged("NumeralAlignment");
			}
		}

		public FontNumeralStyle NumeralStyle
		{
			get { return numeralStyle; }
			set
			{
				if (numeralStyle == value) return;
				numeralStyle = value;
				NotifyPropertyChanged("NumeralStyle");
			}
		}

		public bool Overline
		{
			get { return overline; }
			set
			{
				if (overline == value) return;
				overline = value;
				NotifyPropertyChanged("Overline");
				NotifyPropertyChanged("Decorations");
			}
		}

		public IEnumerable<double> SampleFontSizes
		{
			get
			{
				int i = 0;
				for (; i < FontSizes.Length; i++)
					if (FontSize <= FontSizes[i])
						break;
				if (i < 5)
					i = 0;
				else i = i + 5 > FontSizes.Length ? FontSizes.Length - 10 : i - 5;
				return FontSizes.Skip(i).Take(10);
			}
		}

		private static Typeface SelectBestMatchingTypeface(FontFamily family, FontStyle style, FontWeight weight, FontStretch stretch)
		{
			ICollection<Typeface> typefaces = family.GetTypefaces();
			if (typefaces.Count == 0)
				return null;
			IEnumerable<Typeface> matchingTypefaces
				= from tf in typefaces
				  where tf.Style == style && tf.Weight == weight && tf.Stretch == stretch
				  select tf;
			if (matchingTypefaces.Count() == 0)
				matchingTypefaces = from tf in typefaces
									where tf.Style == style && tf.Weight == weight
									select tf;
			if (matchingTypefaces.Count() == 0)
				matchingTypefaces = from tf in typefaces
									where tf.Style == style
									select tf;
			if (matchingTypefaces.Count() == 0)
				return typefaces.First();
			return matchingTypefaces.First();
		}

		private Typeface SelectBestMatchingTypeface(FontFamily family)
		{
			if (Typeface == null)
			{
				ICollection<Typeface> typefaces = family.GetTypefaces();
				return typefaces.Count == 0 ? null : typefaces.First();
			}
			return SelectBestMatchingTypeface(family, Typeface.Style, Typeface.Weight, Typeface.Stretch);
		}

		public bool SlashedZero
		{
			get { return slashedZero; }
			set
			{
				if (slashedZero == value) return;
				slashedZero = value;
				NotifyPropertyChanged("SlashedZero");
			}
		}

		public bool StandardLigatures
		{
			get { return standardLigatures; }
			set
			{
				if (standardLigatures == value) return;
				standardLigatures = value;
				NotifyPropertyChanged("StandardLigatures");
			}
		}

		public int StandardSwashes
		{
			get { return standardSwashes; }
			set
			{
				if (standardSwashes == value) return;
				standardSwashes = value;
				NotifyPropertyChanged("StandardSwashes");
			}
		}

		public bool Strikethrough
		{
			get { return strikethrough; }
			set
			{
				if (strikethrough == value) return;
				strikethrough = value;
				NotifyPropertyChanged("Strikethrough");
				NotifyPropertyChanged("Decorations");
			}
		}

		public int StylisticAlternates
		{
			get { return stylisticAlternates; }
			set
			{
				if (stylisticAlternates == value) return;
				stylisticAlternates = value;
				NotifyPropertyChanged("StylisticAlternates");
			}
		}

		public bool StylisticSet1
		{
			get { return stylisticSet1; }
			set
			{
				if (stylisticSet1 == value) return;
				stylisticSet1 = value;
				NotifyPropertyChanged("StylisticSet1");
			}
		}

		public bool StylisticSet2
		{
			get { return stylisticSet2; }
			set
			{
				if (stylisticSet2 == value) return;
				stylisticSet2 = value;
				NotifyPropertyChanged("StylisticSet2");
			}
		}

		public bool StylisticSet3
		{
			get { return stylisticSet3; }
			set
			{
				if (stylisticSet3 == value) return;
				stylisticSet3 = value;
				NotifyPropertyChanged("StylisticSet3");
			}
		}

		public bool StylisticSet4
		{
			get { return stylisticSet4; }
			set
			{
				if (stylisticSet4 == value) return;
				stylisticSet4 = value;
				NotifyPropertyChanged("StylisticSet4");
			}
		}

		public bool StylisticSet5
		{
			get { return stylisticSet5; }
			set
			{
				if (stylisticSet5 == value) return;
				stylisticSet5 = value;
				NotifyPropertyChanged("StylisticSet5");
			}
		}

		public bool StylisticSet6
		{
			get { return stylisticSet6; }
			set
			{
				if (stylisticSet6 == value) return;
				stylisticSet6 = value;
				NotifyPropertyChanged("StylisticSet6");
			}
		}

		public bool StylisticSet7
		{
			get { return stylisticSet7; }
			set
			{
				if (stylisticSet7 == value) return;
				stylisticSet7 = value;
				NotifyPropertyChanged("StylisticSet7");
			}
		}

		public bool StylisticSet8
		{
			get { return stylisticSet8; }
			set
			{
				if (stylisticSet8 == value) return;
				stylisticSet8 = value;
				NotifyPropertyChanged("StylisticSet8");
			}
		}

		public bool StylisticSet9
		{
			get { return stylisticSet9; }
			set
			{
				if (stylisticSet9 == value) return;
				stylisticSet9 = value;
				NotifyPropertyChanged("StylisticSet9");
			}
		}

		public bool StylisticSet10
		{
			get { return stylisticSet10; }
			set
			{
				if (stylisticSet10 == value) return;
				stylisticSet10 = value;
				NotifyPropertyChanged("StylisticSet10");
			}
		}

		public bool StylisticSet11
		{
			get { return stylisticSet11; }
			set
			{
				if (stylisticSet11 == value) return;
				stylisticSet11 = value;
				NotifyPropertyChanged("StylisticSet11");
			}
		}

		public bool StylisticSet12
		{
			get { return stylisticSet12; }
			set
			{
				if (stylisticSet12 == value) return;
				stylisticSet12 = value;
				NotifyPropertyChanged("StylisticSet12");
			}
		}

		public bool StylisticSet13
		{
			get { return stylisticSet13; }
			set
			{
				if (stylisticSet13 == value) return;
				stylisticSet13 = value;
				NotifyPropertyChanged("StylisticSet13");
			}
		}

		public bool StylisticSet14
		{
			get { return stylisticSet14; }
			set
			{
				if (stylisticSet14 == value) return;
				stylisticSet14 = value;
				NotifyPropertyChanged("StylisticSet14");
			}
		}

		public bool StylisticSet15
		{
			get { return stylisticSet15; }
			set
			{
				if (stylisticSet15 == value) return;
				stylisticSet15 = value;
				NotifyPropertyChanged("StylisticSet15");
			}
		}

		public bool StylisticSet16
		{
			get { return stylisticSet16; }
			set
			{
				if (stylisticSet16 == value) return;
				stylisticSet16 = value;
				NotifyPropertyChanged("StylisticSet16");
			}
		}

		public bool StylisticSet17
		{
			get { return stylisticSet17; }
			set
			{
				if (stylisticSet17 == value) return;
				stylisticSet17 = value;
				NotifyPropertyChanged("StylisticSet17");
			}
		}

		public bool StylisticSet18
		{
			get { return stylisticSet18; }
			set
			{
				if (stylisticSet18 == value) return;
				stylisticSet18 = value;
				NotifyPropertyChanged("StylisticSet18");
			}
		}

		public bool StylisticSet19
		{
			get { return stylisticSet19; }
			set
			{
				if (stylisticSet19 == value) return;
				stylisticSet19 = value;
				NotifyPropertyChanged("StylisticSet19");
			}
		}

		public bool StylisticSet20
		{
			get { return stylisticSet20; }
			set
			{
				if (stylisticSet20 == value) return;
				stylisticSet20 = value;
				NotifyPropertyChanged("StylisticSet20");
			}
		}

		public Typeface Typeface
		{
			get { return typeface; }
			set
			{
				if (typeface == value) return;
				typeface = value;
				NotifyPropertyChanged("Typeface");
				NotifyPropertyChanged("TypefaceName");
				NotifyPropertyChanged("DisplayName");

				descriptiveTextCultures = null;
				NotifyPropertyChanged("DescriptiveTextCultures");
			}
		}

		public override string ToString()
		{
			return DisplayName;
		}

		public string TypefaceName
		{
			get { return typeface.Name(); }
		}

		public ICollection<Typeface> Typefaces
		{
			get { return FontFamily.GetTypefaces(); }
		}

		public bool Underline
		{
			get { return underline; }
			set
			{
				if (underline == value) return;
				underline = value;
				NotifyPropertyChanged("Underline");
				NotifyPropertyChanged("Decorations");
			}
		}

		public FontVariants Variants
		{
			get { return variants; }
			set
			{
				if (variants == value) return;
				variants = value;
				NotifyPropertyChanged("Variants");
			}
		}

		#region ICloneable Members

		public object Clone()
		{
			return MemberwiseClone();
		}

		#endregion

		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;

		private void NotifyPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion
	}

	public class NamedTypeface
	{
		public NamedTypeface(Typeface typeface)
		{
			Typeface = typeface;
		}

		public Typeface Typeface { get; private set; }

		public string Name
		{
			get { return Typeface.Name(); }
		}
	}
}