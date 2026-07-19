--[[
  Mission: Dudael Breach — Antarctica ice-shelf trigger.
]]

function MissionWorldLoaded()
	if not Guardian or not Nephilim then
		return
	end

	-- IceShelfMarker is a named map actor (see map.yaml).
	if IceShelfMarker then
		MapEvents.ArmDudaelBreach(Nephilim, IceShelfMarker, 4, {
			"AZAZEL.PROXY",
			"AZAZEL.PROXY",
			"AZAZEL.PROXY",
			"AZAZEL.PROXY"
		})
	else
		Media.Debug("Dudael Breach: IceShelfMarker missing from map.yaml")
	end
end
