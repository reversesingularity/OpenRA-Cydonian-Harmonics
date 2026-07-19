--[[
  Operational Silence — Book 3 Ch14 Empyreal Register severance.
  Grants operational-silence, revokes empyreal-link, mirrors silence onto owned spires.
]]

OperationalSilence = {
	Tokens = {},
	SpireTokens = {},
	Active = false
}

local function mirrorSilenceOntoSpires(player, enter)
	local spires = player.GetActorsByType("RESONANCE.SPIRE")
	if enter then
		OperationalSilence.SpireTokens[player.InternalName] = {}
		for i, spire in ipairs(spires) do
			if not spire.IsDead and spire.AcceptsCondition("operational-silence") then
				OperationalSilence.SpireTokens[player.InternalName][i] = {
					Actor = spire,
					Token = spire.GrantCondition("operational-silence")
				}
			end
		end
	else
		local entries = OperationalSilence.SpireTokens[player.InternalName]
		if entries then
			for _, entry in pairs(entries) do
				if entry.Actor and not entry.Actor.IsDead then
					entry.Actor.RevokeCondition(entry.Token)
				end
			end
		end
		OperationalSilence.SpireTokens[player.InternalName] = nil
	end
end

function OperationalSilence.Enter(player)
	if not player or OperationalSilence.Active then
		return
	end

	local state = OperationalSilence.Tokens[player.InternalName] or {}

	if state.EmpyrealLinkToken then
		player.RevokeCondition(state.EmpyrealLinkToken)
		state.EmpyrealLinkToken = nil
	elseif CampaignState and CampaignState.EmpyrealLinkToken then
		player.RevokeCondition(CampaignState.EmpyrealLinkToken)
		CampaignState.EmpyrealLinkToken = nil
	end

	if player.AcceptsCondition("operational-silence") then
		state.SilenceToken = player.GrantCondition("operational-silence")
	end

	OperationalSilence.Tokens[player.InternalName] = state
	OperationalSilence.Active = true
	mirrorSilenceOntoSpires(player, true)

	Media.DisplayMessage("The channel closes. Operational Silence.", "Empyreal Register")
end

function OperationalSilence.Exit(player)
	if not player or not OperationalSilence.Active then
		return
	end

	local state = OperationalSilence.Tokens[player.InternalName] or {}

	if state.SilenceToken then
		player.RevokeCondition(state.SilenceToken)
		state.SilenceToken = nil
	end

	mirrorSilenceOntoSpires(player, false)

	if player.AcceptsCondition("empyreal-link") then
		state.EmpyrealLinkToken = player.GrantCondition("empyreal-link")
		if CampaignState then
			CampaignState.EmpyrealLinkToken = state.EmpyrealLinkToken
		end
	end

	OperationalSilence.Tokens[player.InternalName] = state
	OperationalSilence.Active = false

	Media.DisplayMessage("A thin frequency returns. The link is restored.", "Briefing")
end

function OperationalSilence.IsActive()
	return OperationalSilence.Active
end

-- Prefer EmpyrealMedia so TobitProtocol / Register lines stay muted during silence.
function OperationalSilence.PlaySpeech(player, notification)
	if EmpyrealMedia then
		EmpyrealMedia.PlaySpeechNotification(player, notification)
	else
		if not OperationalSilence.Active then
			Media.PlaySpeechNotification(player, notification)
		end
	end
end
