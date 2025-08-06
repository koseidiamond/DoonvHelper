local customEnemy = require("utils").deepcopy(require("mods").requireFromPlugin("entities.customNPC", "DoonvHelper"))

customEnemy.name = "DoonvHelper/CustomEnemy"

customEnemy.fieldInformation = {
    hitboxHeight = { minimumValue = 0.0, fieldType = "integer" },
    hitboxWidth = { minimumValue = 0.0, fieldType = "integer" },
    hitboxXOffset = { fieldType = "integer" },
    hitboxYOffset = { fieldType = "integer" },
    bounceboxXOffset = { fieldType = "integer" },
    bounceboxYOffset = { fieldType = "integer" }
}

customEnemy.fieldOrder = {
    "x", "y",
    "XSpeed", "YSpeed",
    "acceleration", "jumpHeight",
    "aiType", "spriteID",
    "hitboxWidth", "hitboxHeight",
    "hitboxXOffset", "hitboxYOffset",
    "bounceboxXOffset", "bounceboxYOffset",
    "bounceboxWidth", "bounceboxHeight",
    "facing", "waitForMovement", "outlineEnabled",
    "bulletRecharge", "bulletSpeed",
    "bulletSafeTime", "bulletFacing",
    "bulletSpriteID",
    "health", "deathSound", "shootSound", "dashable"
}
for k, v in pairs({
    health = { fieldType = "integer", minimumValue = 0 },
    bulletRecharge = { minimumValue = 0.0 },
    bulletSafeTime = { minimumValue = 0.0 },
    bounceboxHeight = { minimumValue = 0.0, fieldType = "integer" },
    bounceboxWidth = { minimumValue = 0.0, fieldType = "integer" },
    bulletFacing = customEnemy.fieldInformation["facing"],
}) do customEnemy.fieldInformation[k] = v end
for k, v in pairs({
    health = 1,
    deathSound = "event:/none",
	shootSound = "event:/none",
    bulletSpriteID = "badeline_projectile",
    bulletRecharge = 0.0,
    bulletSpeed = 100.0,
    bulletSafeTime = 0.25,
    bulletFacing = "None",
    dashable = false,
    bounceboxHeight = 0.0,
    bounceboxWidth = 0.0,
    bounceboxXOffset = 0.0,
    bounceboxYOffset = 0.0,
}) do customEnemy.placements.data[k] = v end

return customEnemy
