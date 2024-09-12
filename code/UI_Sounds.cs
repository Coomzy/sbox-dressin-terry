
using Sandbox.Audio;
using Sandbox.UI;

public static class UI_Sounds
{
	public static void Play_OnMouseOver(MousePanelEvent e) => Play_OnMouseOver();
	public static void Play_OnMouseOver()
	{
		var soundHandle = Sound.Play("ui.button.over");
		soundHandle.TargetMixer = Mixer.FindMixerByName("UI");
	}

	public static void Play_OnMouseClick(MousePanelEvent e) => Play_OnMouseClick();
	public static void Play_OnMouseClick()
	{
		var soundHandle = Sound.Play("ui.button.press");
		soundHandle.TargetMixer = Mixer.FindMixerByName("UI");
	}
}