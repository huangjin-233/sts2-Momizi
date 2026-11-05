#nullable enable
using Godot;
using System;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using Momizi.character.power;
using STS2RitsuLib.Scaffolding.Characters;
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 'required' 修饰符或声明为可以为 null。

public partial class Alerts : Control
{ 
	private Creature? _creature;
	private Label _alertLabel;
	private Control _alertcolor;
	private float _expectedMaxFgWidth = -1f;
	private Control _alertcontorl;
	private Control _alert;

	public override void _Ready()
	{
		_alertLabel = GetNode<Label>("%Alertshows");
		_alertcolor = GetNode<Control>("%Alertcolor");
		_alertcontorl = GetNode<Control>("%Alertcontorl");
		_alert = GetNode<Control>("%Alert");
		NCreature? nCreature = GetParent().GetParent() as NCreature;
		_creature = nCreature?.Entity;
	}
	private float MaxFgWidth
	{
		get
		{
			if (!(_expectedMaxFgWidth > 0f))
			{
				return _alertcontorl.Size.X;
			}
			return _expectedMaxFgWidth;
		}
	}
	private float GetFgWidth(int amount)
	{
		return GetFgWidth(amount, MaxFgWidth);
	}
	private float GetFgWidth(int amount, float maxFgWidth)
	{
		if ( _creature != null && _creature.GetPowerAmount<Alert>() <= 0)
		{
			return 0f;
		}
		int max = _creature?.FindPower<Alert>()?.max ?? 20; // 无实例时缺省为上限 20
		float val = (float)amount/max * maxFgWidth;
		return Math.Max(val, _creature != null && ( _creature.CurrentHp > 0) ? 12f : 0f);
	}
	public void RefreshValues()
	{
		RefreshText();
		RefreshForeground();
	}

	public override void _EnterTree()
	{
		CombatManager.Instance.StateTracker.CombatStateChanged += OnCombatStateChanged;
	}

	public override void _ExitTree()
	{
		CombatManager.Instance.StateTracker.CombatStateChanged -= OnCombatStateChanged;
	}
	private void OnCombatStateChanged(CombatState _)
	{
		_alert.Visible = true;
		RefreshValues();
		RefreshForeground();
	}
	private void RefreshText()
	{
		_alertLabel.Visible = true;
		if (_creature != null) _alertLabel.Text = _creature.GetPowerAmount<Alert>() + "/" + (_creature.FindPower<Alert>()?.max ?? 20);
	}

	private void RefreshForeground()
	{
		_alertcolor.Visible = true;
		if (_creature != null) _alertcolor.OffsetRight = GetFgWidth(_creature.GetPowerAmount<Alert>()) + 10f;
	}
}
