﻿using System;
using UnityEngine;
using Verse;
using RimWorld;

using static ZiTools.StaticConstructor;

namespace ZiTools
{
	public class ObjectSeeker_Window : Window
	{
		Vector2 ScrollPosition;

		static Action UpdateAction = delegate { };

		public ObjectSeeker_Window() : base()
		{
			this.doCloseX = true;
			this.preventDrawTutor = true;
			this.draggable = true;
			this.preventCameraMotion = false;

			this.ScrollPosition = new Vector2();
		}

		public override void PreOpen()
		{
			base.PreOpen();
			Clear();
			Update();
			OSD_Global.WindowIsOpen = true;
		}

		public override void DoWindowContents(Rect inRect)
		{
			Text.Font = GameFont.Medium;
			Rect titleRect = new Rect(inRect) { height = Text.LineHeight + 7f };
			Rect updateButtonRect = new Rect(titleRect.x, titleRect.yMax, titleRect.width / 2f - 2f, 20f);
			Rect clearButtonRect = updateButtonRect;
			clearButtonRect.x = updateButtonRect.xMax + 4f;

			Widgets.Label(titleRect, "ZiT_ObjectsSeekerLabel".Translate());
			Text.Font = GameFont.Small;

			if (Widgets.ButtonText(updateButtonRect, "ZiT_UpdateButtonLabel".Translate()))
			{
				Update();
			}
			if (Widgets.ButtonText(clearButtonRect, "ZiT_ClearButtonLabel".Translate()))
			{
				Clear();
			}

			float curY = updateButtonRect.yMax;
			string partOfCategoryLabel = "ZiT_CategoryLabel".Translate() + ": ";
			Widgets.ListSeparator(ref curY, inRect.width, partOfCategoryLabel + OSD_Global.SelectedCategoryName);
			Rect categoryButtonRect = new Rect(inRect.x, updateButtonRect.yMax, inRect.width, curY - updateButtonRect.yMax);
			if (Widgets.ButtonInvisible(categoryButtonRect))
				Find.WindowStack.Add(OSD_Global.CategoryMenu);
			Widgets.DrawHighlightIfMouseover(categoryButtonRect);

			if (!OSD_Global.CategoriesDict.ContainsKey(OSD_Global.SelectedCategory))
			{
				Widgets.Label(new Rect(titleRect) { y = curY }, "ZiT_NotFoundString".Translate(OSD_Global.SelectedCategoryName));
				return;
			}
			Rect mainRect = new Rect(inRect) { yMin = curY };
			Rect rect1 = new Rect(0.0f, 0.0f, mainRect.width - 16f, (OSD_Global.CategoriesDict[OSD_Global.SelectedCategory].Count + 1) * Text.LineHeight);
			curY = rect1.y;

			Widgets.BeginScrollView(mainRect, ref ScrollPosition, rect1, true);
			GUI.BeginGroup(rect1);
			curY += this.GroupOfThingsMaker(rect1.x, curY, rect1.width, "ZiT_NameLabel".Translate(), OSD_Global.SelectedCategory == ObjectSeeker_Data.CategoryOfObjects.Corpses ? "ZiT_TimeUntilRotted".Translate() : "ZiT_CellsCountLabel".Translate(), false);

			OSD_Global.CategoriesDict[OSD_Global.SelectedCategory].Sort();
			if (OSD_Global.SelectedCategory == ObjectSeeker_Data.CategoryOfObjects.Corpses)
				OSD_Global.CategoriesDict[OSD_Global.SelectedCategory].Sort(OSD_Global);
			foreach (string currentName in OSD_Global.CategoriesDict[OSD_Global.SelectedCategory])
			{
				string param = OSD_Global.SelectedCategory == ObjectSeeker_Data.CategoryOfObjects.Corpses ? (OSD_Global.CorpsesTimeRemainDict[currentName] > 0 ? OSD_Global.CorpsesTimeRemainDict[currentName].ToStringTicksToDays() : "-") : OSD_Global.LocationsDict[currentName]?.Count.ToString();
				curY += this.GroupOfThingsMaker(rect1.x, curY, rect1.width, currentName, param);
			}
			GUI.EndGroup();
			Widgets.EndScrollView();
		}

		public override void PreClose()
		{
			base.PreClose();
			Clear();
			OSD_Global.WindowIsOpen = false;
		}

		public static void SetUpdateAction(Action action) => UpdateAction += action;

		public static void ClearUpdateAction() => UpdateAction = delegate { };

		public override Vector2 InitialSize { get => new Vector2(250f, 365f); }

		public static void DrawWindow() => Find.WindowStack.Add(new ObjectSeeker_Window());

		float GroupOfThingsMaker(float x, float y, float width, string label, string param, bool createFindButton = true)
		{
			Rect rectLabel = new Rect(x, y, width, Text.LineHeight);

			if (label == OSD_Global.ThingToSeek)
				Widgets.DrawHighlightSelected(rectLabel);
			else if(createFindButton)
			{
				if (Widgets.ButtonInvisible(rectLabel))
				{
					OSD_Global.ThingToSeek = label;
					MapMarksManager.SetMarks(MapMarksManager.ObjectSeeker_MarkDef);
					UpdateAction();
				}
				Widgets.DrawHighlightIfMouseover(rectLabel);
			}
			
			Rect rectParam = new Rect(width - Text.CalcSize(param).x, y, Text.CalcSize(param).x, Text.LineHeight);
			Widgets.Label(rectLabel.RightPartPixels(rectParam.width), param);

			rectLabel.width -= rectParam.width;
			Widgets.Label(rectLabel.LeftPartPixels(rectLabel.width), label);
			TooltipHandler.TipRegion(rectLabel, label.ToString());
			return rectLabel.height;
		}

		public static void Update()
		{
			OSD_Global.FindAllThings();
			MapMarksManager.SetMarks(MapMarksManager.ObjectSeeker_MarkDef);
			UpdateAction();
		}

		void Clear()
		{
			OSD_Global.ThingToSeek = string.Empty;
			MapMarksManager.RemoveMarks(MapMarksManager.ObjectSeeker_MarkDef);
			UpdateAction();
		}
	}
}
