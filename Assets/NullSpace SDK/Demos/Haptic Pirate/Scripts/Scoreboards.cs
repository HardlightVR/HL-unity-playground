using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NullSpace.SDK.Demos
{
	public class Scoreboards : MonoBehaviour
	{
		[SerializeField]
		private List<Text> _textField;
		public List<Text> TextField
		{
			get
			{
				if (_textField == null)
				{
					_textField = new List<Text>();
				}
				return _textField;
			}
			set
			{
				if (value != null)
				{
					_textField = value;
				}
			}
		}

		public bool NeedUpdateColor;
		private Color ProfitColor = new Color32(0, 255, 76, 255);
		private Color DebtColor = new Color32(255, 0, 0, 255);
		public bool Profiting = false;
		private Color _textColor = Color.white;
		public Color TextColor
		{
			get
			{
				return _textColor;
			}
			set
			{
				if (value != _textColor)
				{
					NeedUpdateColor = true;
					_textColor = value;
				}
			}
		}
		public int currentScore;

		public void AddMoney(int gained)
		{
			currentScore += gained;

			if (Profiting && currentScore < 0)
			{
				TextColor = DebtColor;
				Profiting = false;
			}
			else if (!Profiting && currentScore >= 0)
			{
				TextColor = ProfitColor;
				Profiting = true;
			}

			UpdateAllScoreboards(true, NeedUpdateColor);
		}

		public void UpdateAllScoreboards(bool updateText, bool updateColor)
		{
			for (int i = 0; i < TextField.Count; i++)
			{
				if (updateText)
				{
					UpdateScoreboardText(TextField[i]);
				}
				if (updateColor)
				{
					UpdateScoreboardTextColor(TextField[i]);
				}
			}
		}

		public void UpdateScoreboardText(Text scoreboard)
		{
			scoreboard.text = "$" + currentScore + "00";
		}

		public void UpdateScoreboardTextColor(Text scoreboard)
		{
			scoreboard.color = TextColor;
			NeedUpdateColor = false;
		}

		void Start()
		{
			TextColor = ProfitColor;
			Profiting = false;
			currentScore = 0;
			UpdateAllScoreboards(true, true);
		}
	}
}