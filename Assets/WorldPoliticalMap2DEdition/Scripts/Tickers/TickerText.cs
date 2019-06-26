using UnityEngine;
using System;
using System.Collections;

namespace WPMF
{
	[Serializable]
	public class TickerText: ICloneable
	{
	
		/// <summary>
		/// On which ticker line should the text be put (0..NUM_TICKERS)
		/// </summary>
		public int tickerLine;
		public string text = "Ticker";
	
		/// <summary>
		/// Fade in/out duration in seconds. Set it to zero to disable fade effect.
		/// </summary>
		public float fadeDuration = 0.5f;

		/// <summary>
		/// If set to 0, text will last for ever or until it finishes scrolling.
		/// </summary>
		public float duration = 5.0f;
		[SerializeField]
		float
			_horizontalOffset;
		/// <summary>
		/// Starting position (-0.5..0.5).
		/// Only used if scrollSpeed is zero, otherwise it will be ignored and new texts will automatically enter from the corresponding initial edge.
		/// </summary>
		public float horizontalOffset {
			get { return _horizontalOffset; }
			set {
				if (_horizontalOffset != value) {
					_horizontalOffset = value; 
					if (gameObject != null) {
						gameObject.transform.localPosition = new Vector3 (_horizontalOffset, gameObject.transform.localPosition.y, gameObject.transform.localPosition.z);
					}
				}
			}
		}

		/// <summary>
		/// Optional scaling factor for text, useful if using different fonts.
		/// </summary>
		public float textScale = 1.0f;

		/// <summary>
		/// Set it to 0 to disable blinking effect.
		/// </summary>
		public float blinkInterval = 0;
	
		/// <summary>
		/// The blink repetitions. A 0 will blink indefinitely (as long as blinkInterval>0)
		/// </summary>
		public int blinkRepetitions = 0;
		[SerializeField]
		Color
			_textColor = Color.white;

		public Color textColor {
			get {
				return _textColor; 
			}
			set {
				if (_textColor != value) {
					_textColor = value;
					if (gameObject != null) {
						gameObject.GetComponent<TextMesh> ().color = _textColor;
					}
				}
			}
		}

		public bool drawTextShadow = true;
		[SerializeField]
		Color
			_shadowColor = Color.black;
		
		public Color shadowColor {
			get {
				return _shadowColor; 
			}
			set {
				if (_shadowColor != value) {
					_shadowColor = value;
					if (gameObject != null) {
						gameObject.transform.Find ("shadow").GetComponent<TextMesh> ().color = _shadowColor;
					}
				}
			}
		}

		[SerializeField]
		Font
			_font;
		/// <summary>
		/// The font for the ticket text (null will use the default, which is Lato and is provided in Resources/Font)
		/// </summary>
		public Font font {
			get {
				return _font;
			}
			set {
				if (_font != value) {
					_font = value;
					if (_font != null) {
						Material fontMaterial = GameObject.Instantiate (Resources.Load<Material>("WPMF/Materials/Font")); // this material is linked to a shader that has into account zbuffer
						fontMaterial.mainTexture = _font.material.mainTexture;
						fontMaterial.hideFlags = HideFlags.DontSave;
						fontMaterial.renderQueue += 5;
						_font.material = fontMaterial;
						_shadowMaterial = GameObject.Instantiate (_font.material);
						_shadowMaterial.hideFlags = HideFlags.DontSave;
						_shadowMaterial.renderQueue--;
					} else {
						_shadowMaterial = null;
					}
				}

			}
		}

		[SerializeField]
		Material
			_shadowMaterial;
		/// <summary>
		/// The shadow font material. It null, it will create it automatically.
		/// </summary>
		public Material shadowMaterial { get { return _shadowMaterial; } }
	
		/// <summary>
		/// Reference to the TextMesh object once the ticker has been created on the scene
		/// </summary>
		[NonSerialized]
		public GameObject
			gameObject;

		/// <summary>
		/// The size of the text mesh once created.
		/// </summary>
		[NonSerialized]
		public Vector3
			textMeshSize;

		public TickerText ()
		{
		}

		public TickerText (int tickerLine, string text)
		{
			this.tickerLine = tickerLine;
			this.text = text;
		}

		public object Clone ()
		{
			TickerText clone = new TickerText ();
			clone.blinkInterval = this.blinkInterval;
			clone.blinkRepetitions = this.blinkRepetitions;
			clone.drawTextShadow = this.drawTextShadow;
			clone.duration = this.duration;
			clone.fadeDuration = this.fadeDuration;
			clone.font = this.font;
			clone.horizontalOffset = this.horizontalOffset;
			clone.shadowColor = this.shadowColor;
			clone.text = this.text;
			clone.textColor = this.textColor;
			clone.tickerLine = this.tickerLine;
			return clone;
		}
	
	
	}
}