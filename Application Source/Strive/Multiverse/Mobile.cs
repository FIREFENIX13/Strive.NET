using System;

namespace Strive.Multiverse
{
	/// <summary>
	/// Summary description for Mobile.
	/// </summary>
	public class Mobile : PhysicalObject
	{
		public int Level;
		public int Cognition;
		public int Willpower;
		public int Dexterity;
		public int Strength;
		public int Constitution;
		public EnumRace Race;
		public float HitPoints;
		int MaxHitPoints;
		public EnumMobileSize MobileSize;
		public EnumMobileState MobileState;

		public Mobile (
			Schema.TemplateMobileRow mobile,
			Schema.ObjectTemplateRow template,
			Schema.ObjectInstanceRow instance
		) : base ( template, instance ) {
			Level = mobile.Level;
			Cognition = mobile.Cognition;
			Willpower = mobile.Willpower;
			Dexterity = mobile.Dexterity;
			Strength = mobile.Strength;
			Constitution = mobile.Constitution;
			Race = (EnumRace)mobile.EnumRaceID;
			MobileSize = (EnumMobileSize)mobile.EnumMobileSizeID;
			MobileState = (EnumMobileState)mobile.EnumMobileStateID;
			MaxHitPoints = mobile.EnumMobileSizeID*100 + Level * Constitution / 2;
			HitPoints = (int)MaxHitPoints;
		}
	}
}
