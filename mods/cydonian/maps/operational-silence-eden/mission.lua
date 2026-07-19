--[[
  Mission: Operational Silence — Eden departure (Book 3 Ch14 beat).
]]

function MissionWorldLoaded()
	if not Guardian then
		return
	end

	MapEvents.StartEdenDilationWatch(Guardian)

	-- After a short grace period, sever the Empyreal Register (Operational Silence).
	Trigger.AfterDelay(DateTime.Seconds(45), function()
		OperationalSilence.Enter(Guardian)
	end)

	-- Optional Tobit Exception beat later in the campaign would call OperationalSilence.Exit.
	Trigger.AfterDelay(DateTime.Minutes(3), function()
		if OperationalSilence.IsActive() then
			OperationalSilence.Exit(Guardian)
		end
	end)
end
