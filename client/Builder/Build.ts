import { LoadModel } from "../util.js"
import { BuildEntity } from "./BuildEntity.js"

export const ActiveBuild = (() => {
    let instance: Build 

    const get = () => {
        return instance
    }
    const set = ( build: Build ) => {
        instance = build
    }


    return { get, set }
})()

export default class Build {
    RootEntity: BuildEntity
    Name: string = "Untitled Build"
    Author: string = ""
    Created: number = Date.now()

    static async StartNewStructure() {
        // TODO: in future, start prop selection?
        const coneHash = await LoadModel( "prop_roadcone02a" )
        const pos = GetOffsetFromEntityInWorldCoords(GetPlayerPed(-1), 0, 5, 0)
        const cone = CreateObject( coneHash, pos[0], pos[1], pos[2], true, true, true )
        if ( !cone ) throw new Error( "Failed to spawn entity (null handle)" )
        
        const build = new Build( cone )
        if ( !ActiveBuild.get() ) ActiveBuild.set(build)
        return build
    }

    constructor( parentEntity: number ) {
        this.RootEntity = new BuildEntity(parentEntity)
    }

}