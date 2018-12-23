package.path = package.path .. ";?.lua"
ZBS = "D:/work/ZeroBraneStudioEduPack-1.80-win32/"
package.path = package.path ..";"..ZBS.."lualibs/?/?.lua;"..ZBS.."lualibs/?.lua"
debugMode = true

function namespace(ns) 
	-- do nothing
end

function include(name)
	local i = string.find(name, "%.%a+$")
	if i then
		return require("@"..string.sub(name, i+1))
	end
	return require("#"..name)
end

if debugMode then
	local mobdebug = require("mobdebug")
	mobdebug.start()
	print "debugMode is On"
end

-- local typeMeta = {}
-- typeMeta.__index = function(table, name)
-- 	local t = rawget(table, name)
-- 	if not t then
-- 		t = require("@"..name)
-- 		if t then
-- 			rawset(table, name, t)
-- 		end
-- 	end
-- 	return t
-- end

-- local typeTable = {}
-- setmetatable(typeTable, typeMeta)

-- _G.TYPE = typeTable

math.randomseed(os.time())

require "Global"