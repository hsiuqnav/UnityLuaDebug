namespace "Test"
local Entity = include("Test.Entity")

local TestManager = Class:extend
{
	
}

function TestManager.TestAdd(lhs, rhs)
	local temp = lhs * 100 + rhs
	local result = (temp + 33) % 5
	print("TestAdd " .. lhs .. " " .. rhs .. " result " .. result)

	local entitya = Entity()
	entitya:PrintNum()

	local entityb = Entity()
	entityb:SetNum(255)
	entityb:PrintNum()
end

return TestManager