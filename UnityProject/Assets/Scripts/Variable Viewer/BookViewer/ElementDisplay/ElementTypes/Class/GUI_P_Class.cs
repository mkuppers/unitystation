﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Messages.Client.VariableViewer;


namespace AdminTools.VariableViewer
{
	public class GUI_P_Class : PageElement
	{
		public override PageElementEnum PageElementType => PageElementEnum.Class;

		public Button TButton;
		public TMP_Text TText;
		public bool IsSentence;
		public bool iskey;

		public override bool IsThisType(Type TType)
		{
			if (!TType.IsValueType && !(TType == typeof(string)))
			{
				return true;
			}
			//Need testing
			else if (TType.IsValueType && !TType.IsPrimitive && !(TType == typeof(string)))
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public override void SetUpValues(
				Type ValueType, VariableViewerNetworking.NetFriendlyPage Page = null,
				VariableViewerNetworking.NetFriendlySentence Sentence = null, bool Iskey = false)
		{
			var name = VVUIElementHandler.ReturnCorrectString(Page, Sentence, Iskey);
			if (name.Length > 50)
				name = name.Substring(0, 50);
			TText.text = name;
			if (Page != null)
			{
				PageID = Page.ID;
				SentenceID = 0;
				IsSentence = false;
				iskey = false;
			}
			else
			{
				PageID = Sentence.OnPageID;
				SentenceID = Sentence.SentenceID;
				IsSentence = true;
				iskey = Iskey;
			}
		}

		public override void Pool()
		{
			IsSentence = false;
			iskey = false;
		}

		public void RequestOpenBookOnPage()
		{
			OpenPageValueNetMessage.Send(PageID, SentenceID, IsSentence, iskey);
		}
	}
}
