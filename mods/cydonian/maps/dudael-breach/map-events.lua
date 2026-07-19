--[[
  Dudael Breach — spawn Azazel's biomechanical swarms on ice-shelf proximity.
]]

MapEvents = {
	BreachTriggered = false,
	DefaultSwarm = { "AZAZEL.PROXY", "AZAZEL.PROXY", "AZAZEL.PROXY" }
}

function MapEvents.ArmDudaelBreach(owner, triggerActor, rangeCells, swarmTypes)
	if not owner or not triggerActor then
		return
	end

	local types = swarmTypes or MapEvents.DefaultSwarm
	local range = WDist.FromCells(rangeCells or 3)

	Trigger.OnEnteredProximityTrigger(triggerActor.CenterPosition, range, function(actor, id)
		if MapEvents.BreachTriggered then
			return
		end

		-- Only the Guardian expedition trips the shelf.
		if actor.Owner ~= Guardian then
			return
		end

		MapEvents.BreachTriggered = true
		Trigger.RemoveProximityTrigger(id)
		MapEvents.SpawnDudaelSwarm(owner, triggerActor.Location, types)
	end)
end

function MapEvents.SpawnDudaelSwarm(owner, originCell, swarmTypes)
	Media.DisplayMessage("The ice shelf ruptures. Dudael is breached.", "Briefing")

	local delay = 0
	for i, unitType in ipairs(swarmTypes) do
		local offset = i - 1
		Trigger.AfterDelay(delay, function()
			local cell = CPos.New(originCell.X + offset, originCell.Y)
			Actor.Create(unitType, true, { Owner = owner, Location = cell })
		end)
		delay = delay + DateTime.Seconds(1)
	end
end

-- Eden dilation stubs unused on this map.
function MapEvents.StartEdenDilationWatch(player) end
function MapEvents.EnterEdenDilation(player) end
function MapEvents.ExitEdenDilation(player) end
