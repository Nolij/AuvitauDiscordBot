local MoneyLib = {}
MoneyLib.Suffixes = {
	"k",
	"M",
	"B",
	"T",
	"qd",
	"Qn",
	"sx",
	"Sp",
	"O",
	"N",
	"de",
	"Ud",
	"DD",
	"tdD",
	"qdD",
	"QnD",
	"sxD",
	"SpD",
	"OcD",
	"NvD",
	"Vgn",
	"UVg",
	"DVg",
	"TVg",
	"qtV",
	"QnV",
	"SeV",
	"SPG",
	"OVG",
	"NVG",
	"TGN",
	"UTG",
	"DTG",
	"tsTG",
	"qtTG",
	"QnTG",
	"ssTG",
	"SpTG",
	"OcTG",
	"NoTG",
	"QdDR",
	"uQDR",
	"dQDR",
	"tQDR",
	"qdQDR",
	"QnQDR",
	"sxQDR",
	"SpQDR",
	"OQDDr",
	"NQDDr",
	"qQGNT",
	"uQGNT",
	"dQGNT",
	"tQGNT",
	"qdQGNT",
	"QnQGNT",
	"sxQGNT",
	"SpQGNT",
	"OQQGNT",
	"NQQGNT",
	"SXGNTL"
}
function MoneyLib.HandleMoney(Input)
	Input = tonumber(Input)
	local Negative = Input < 0
	Input = math.abs(Input)
	local Paired = false
	for i, v in pairs(MoneyLib.Suffixes) do
		if not (Input >= 10 ^ (3 * i)) then
			Input = Input / 10 ^ (3 * (i - 1))
			local isComplex = string.find(tostring(Input), ".") and string.sub(tostring(Input), 4, 4) ~= "."
			Input = string.sub(tostring(Input), 1, isComplex and 4 or 3) .. (MoneyLib.Suffixes[i - 1] or "")
			Paired = true
			break
		end
	end
	if not Paired then
		local Rounded = math.floor(Input)
		Input = tostring(Rounded)
	end
	if Negative then
		return "(-" .. Input .. ")"
	end
	return Input
end
function MoneyLib.UnHandleMoney(Input)
	Input = tostring(Input)
	local Negative = Input:find("-") and -1 or 1
	local Suffix = (Input:match("[abcdefghijklmnopqrstuvqxyzABCDEFGHIJKLMNOPQRSTUVWXYZ]+") or ""):lower()
	local Number = Input:match("[0123456789%.]+")
	local Index = -1
	for i,v in next, MoneyLib.Suffixes do
		if v:lower() == Suffix then
			Index = i
			break
		end
	end
	local Output = nil
	if Index > 0 then
		Output = Negative * tonumber(Number) * (10 ^ (3 * Index))
	else
		Output = Negative * tonumber(Number)
	end
	return Output
end
_G.MoneyLib = MoneyLib