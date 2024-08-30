import { Colors, Vector3 } from "../shared/types.js"
import { DrawEntityBox, DrawText3D } from "./DrawUtil.js"
import { Delay, RemoveAllAttachments } from "./util.js"
//@ts-expect-error 
import { AbortController } from "./Abort.js"

let previewData: { entity: number, name: string, timer: number, _tick: number } | null = null

export function previewEntity( entityHandle: number, name?: string ) {
    clearPreview()
    SetEntityAlpha( entityHandle, 180, false )
    SetEntityCollision( entityHandle, false, false )
    SetEntityCompletelyDisableCollision( entityHandle, false, false )
    // FreezeEntityPosition( entityHandle, true )
    SetEntityInvincible( entityHandle, true )
    SetEntityHasGravity( entityHandle, false )
    if ( IsEntityAVehicle( entityHandle ) ) {
        DisableVehicleWorldCollision( entityHandle )
        SetVehicleGravity(entityHandle, false)
    }
    previewData = {
        entity: entityHandle,
        name: name ?? entityHandle.toString(),
        timer: setTick( tickPreview ),
        _tick: 0
    }
}

let _previewSpawnAbortController: AbortController
async function createPreviewEntity( modelId: string, creationFn: ( hash: number, pos: Vector3 ) => number ) {
    const hash = GetHashKey( modelId )
    if ( !IsModelValid( hash ) ) return null

    // Setup a new abort controller, to let us cancel previous spawn requests
    if ( _previewSpawnAbortController != null ) {
        _previewSpawnAbortController.abort( "previewProp" )
    }
    _previewSpawnAbortController = new AbortController()
    const signal = _previewSpawnAbortController.signal

    // Load the model, with cancellation support
    RequestModel( hash )
    while ( !HasModelLoaded( hash ) && !signal.aborted ) {
        await Delay( 200 )
    }
    if(signal.aborted) return null

    const pos = GetOffsetFromEntityInWorldCoords( GetPlayerPed( -1 ), 0, 5, 0 ) as Vector3
    const entity = creationFn( hash, pos )
    if(!entity) throw new Error("Entity failed to be created")
    previewEntity( entity, modelId )
    SetModelAsNoLongerNeeded( hash )
    return entity
}
/**
 * Spawns a preview prop with given model ID
 * @param modelId the model string name
 * @returns the entity handle, or null if failed (model invalid)
 */
export async function previewProp( modelId: string ): Promise<number | null> {
    return createPreviewEntity( modelId, ( hash: number, pos: Vector3 ) => {
        return CreateObject( hash, pos[0], pos[1], pos[2], false, false, false )
    })
}
export async function previewVehicle( modelId: string ): Promise<number | null> {
    return createPreviewEntity( modelId, ( hash: number, pos: Vector3 ) => {
        const heading = GetEntityHeading(GetPlayerPed(-1))
        return CreateVehicle( hash, pos[0], pos[1], pos[2], heading, false, false)
    } )
}
export async function previewPed( modelId: string ): Promise<number | null> {
    return createPreviewEntity( modelId, ( hash: number, pos: Vector3 ) => {
        const heading = GetEntityHeading( GetPlayerPed( -1 ) )
        return CreatePed( 0, hash, pos[0], pos[1], pos[2], heading, false, false )
    } )
}
export function clearPreview() {
    if ( !previewData ) return
    clearTick( previewData.timer )
    RemoveAllAttachments(previewData.entity)
    DeleteEntity(previewData.entity)
    previewData = null
}

export function tickPreview() {
    if ( !previewData ) return
    SetEntityVelocity( previewData.entity, 0, 0, 0 )
    const pos = GetOffsetFromEntityInWorldCoords(GetPlayerPed(-1), 0, 5, 0.3) as Vector3
    SetEntityCoords( previewData.entity, pos[0], pos[1], pos[2], true, true, false, false )

    DrawText3D( pos, previewData.name, 0.2 )
    DrawEntityBox( previewData.entity, Colors.Red )
    
    // if ( previewData._tick > 2 ) {
        previewData._tick = 0;
        const heading = GetEntityHeading( previewData.entity )
        SetEntityRotation(previewData.entity, 0, 0, heading + 1, 2, false)
    // }

}