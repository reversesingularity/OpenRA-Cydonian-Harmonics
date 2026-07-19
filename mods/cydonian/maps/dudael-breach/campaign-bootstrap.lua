--[[
  Campaign bootstrap — Dudael Breach (Antarctica / Ross Ice Shelf).
]]

Guardian = Player.GetPlayer("Multi0")
Nephilim = Player.GetPlayer("Creeps")

function WorldLoaded()
	if Guardian and Guardian.AcceptsCondition("empyreal-link") then
		CampaignState = CampaignState or {}
		CampaignState.EmpyrealLinkToken = Guardian.GrantCondition("empyreal-link")
	end

	Media.DisplayMessage("Dudael stirs beneath the ice. Azazel's prison thins.", "Briefing")

	if MissionWorldLoaded then
		MissionWorldLoaded()
	end
end
