namespace "Base"

local Class = { }

Class.__index = Class

function Class:extend(static)
    local class = static or {}
    setmetatable(class, self)
    class.__call = self.__call
    class.__index = class
    return class
end

function Class:__call(...)
    local object = {}
    setmetatable(object, self)
    if self.__init then
        self.__init(object, ...)
    end
    return object
end

function Class:CreateAndInit(...)
	local params = {...}
	return function(object)
		setmetatable(object, self)
		if self.__init then
			self.__init(object, table.unpack(params))
		end
		return object
	end
end

function Class:IsSubclassOf(class)
	local mine = self.TYPE_NAME
	local other = class.TYPE_NAME
	
	if mine and other then
		if mine == other then
			return true
		end
		
		local mineType = getmetatable(self)
		if mineType and mineType.IsSubclassOf then
			return mineType:IsSubclassOf(class)
		end
	end
	return false
end

function Class:IsTypeOfInstance(object)
	if object then
		local class = getmetatable(object)
		if class and class.IsSubclassOf then
			return class:IsSubclassOf(self)
		end
	end
	return false
end

return Class