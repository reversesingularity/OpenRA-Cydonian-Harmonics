--[[
  Shared dynamic map events — Eden time dilation via DateTime.GameTime.
  Dudael Breach helpers live in the dudael-breach map copy of this file.
]]

MapEvents = {
	DilationToken = nil,
	DilationActive = false,
	-- Ticks after mission start before Eden dilation engages (Narnia-style compression).
	DilationThreshold = DateTime.Seconds(30),
	DilationDuration = DateTime.Minutes(2)
}

function MapEvents.StartEdenDilationWatch(player)
	MapEvents.DilationStart = DateTime.GameTime
	MapEvents.DilationPlayer = player

	Trigger.AfterDelay(MapEvents.DilationThreshold, function()
		MapEvents.EnterEdenDilation(player)
	end)
end

function MapEvents.EnterEdenDilation(player)
	if MapEvents.DilationActive or not player then
		return
	end

	if player.AcceptsCondition("eden-dilated") then
		MapEvents.DilationToken = player.GrantCondition("eden-dilated")
		MapEvents.DilationActive = true
		Media.DisplayMessage("Time folds. Resonance accrues under Eden dilation.", "Briefing")

		Trigger.AfterDelay(MapEvents.DilationDuration, function()
			MapEvents.ExitEdenDilation(player)
		end)
	end
end

function MapEvents.ExitEdenDilation(player)
	if not MapEvents.DilationActive or not player then
		return
	end

	if MapEvents.DilationToken then
		player.RevokeCondition(MapEvents.DilationToken)
		MapEvents.DilationToken = nil
	end

	MapEvents.DilationActive = false
	Media.DisplayMessage("Eden's dilation fades. Clocks align with the outer war.", "Briefing")
end

function MapEvents.ArmDudaelBreach(owner, triggerPos, range, swarmTypes)
	Media.Debug("MapEvents.ArmDudaelBreach: use dudael-breach map-events.lua")
end
