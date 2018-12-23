namespace "Test"

local Entity = Class:extend
{
}

function Entity:SetNum(num)
	self.Num = num
end

function Entity:PrintNum()
	print(self.Num or 0)
end

return Entity