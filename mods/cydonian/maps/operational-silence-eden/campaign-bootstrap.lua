--[[
  Campaign bootstrap — resolves players and opens the Empyreal link.
  Loaded before mission.lua on campaign maps.
]]

Guardian = Player.GetPlayer("Multi0")
Nephilim = Player.GetPlayer("Creeps")

function WorldLoaded()
	if Guardian then
		-- Default: Empyreal channel open until Operational Silence severs it.
		if Guardian.AcceptsCondition("empyreal-link") then
			CampaignState = CampaignState or {}
			CampaignState.EmpyrealLinkToken = Guardian.GrantCondition("empyreal-link")
		end
	end

	Media.DisplayMessage("Eden holds. The Empyreal Register still answers.", "Briefing")

	if MissionWorldLoaded then
		MissionWorldLoaded()
	end
end
