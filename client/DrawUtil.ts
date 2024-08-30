import { Vector2, Vector3, Color } from '../shared/types';
export function GetScreenCoords( worldPos: Vector3 ) : Vector2 | null
{
    const [success, x, y] = GetScreenCoordFromWorldCoord( worldPos[0], worldPos[1], worldPos[2] );
    return success ? [x, y] : null
}

export function DrawLine( from: Vector3, to: Vector3, color: Color ) {
    global.DrawLine( from[0], from[1], from[2], to[0], to[1], to[2], color[0], color[1], color[2], color[3] ?? 255 )
}

export function DrawSphere( pos: Vector3, radius: number, color: Color = [255, 255, 255, 255] ) {
    global.DrawMarker(28, pos[0], pos[1], pos[2], 0, 0, 0, 0, 0, 0, radius, radius, radius, color[0], color[1], color[2], color[3] ?? 255, false, false, 2, false, "", "", false );
}

export function DrawBox( cornerA: Vector3, cornerB: Vector3, color: Color ) {
    global.DrawBox( cornerA[0], cornerA[1], cornerA[2], cornerB[0], cornerB[1], cornerB[2], color[0], color[1], color[2], color[3] ?? 255 );
}

const Deg2Rad = Math.PI / 180;
export function DrawBoundingBoxFromCenter( center: Vector3, size: Vector3, rotation: Vector3, color: Color ) {
    const points: Vector3[] = [
        [-size[0], -size[1], size[2]],
        [size[0], -size[1], size[2]],
        [size[0], size[1], size[2]],
        [-size[0], size[1], size[2]],

        [-size[0], size[1], -size[2]],
        [size[0], size[1], -size[2]],
        [size[0], -size[1], -size[2]],
        [-size[0], -size[1], -size[2]]
    ]

    const rotX = rotation[0] * Deg2Rad;
    const rotY = rotation[1] * Deg2Rad;
    const rotZ = rotation[2] * Deg2Rad;
    for ( let i = 0; i < 8; i++ ) {
        const [x, y, z] = points[i]
        points[i][0] = x * Cos( rotY ) * Cos( rotZ )
            + y * ( Cos( rotZ ) * Sin( rotX ) * Sin( rotY ) - Cos( rotX ) * Sin( rotZ ) )
            + z * ( Cos( rotX ) * Cos( rotZ ) * Sin( rotY ) + Sin( rotX ) * Sin( rotZ ) );

        points[i][1] = x * Cos( rotY ) * Sin( rotZ )
            + z * ( -Cos( rotZ ) * Sin( rotX ) + Cos( rotX ) * Sin( rotY ) * Sin( rotZ ) )
            + y * ( Cos( rotX ) * Cos( rotZ ) + Sin( rotX ) * Sin( rotY ) * Sin( rotZ ) );

        points[i][2] = z * Cos( rotX ) * Cos( rotY )
            + y * Cos( rotY ) * Sin( rotX )
            - x * Sin( rotY );
        // Align with center pos
        points[i][0] += center[0];
        points[i][1] += center[1];
        points[i][2] += center[2];
    }

    for ( let i = 0; i < 4; i++) {
        // Form line for upper
        DrawLine( points[i], points[i + 1], color );
        // Connect upper to lower
        DrawLine( points[i], points[7 - i], color );
        DrawLine( points[i + 3], points[i + 4], color );
    }
    DrawLine( points[0], points[3], color );
    DrawLine( points[7], points[4], color );
}
        
export function DrawBoxFromCenter( center: Vector3, size: Vector3, color: Color ) {
    const cornerA = [ center[0] - size[0], center[1] - size[1], center[2] - size[2] ]
    const cornerB = [ center[0] + size[0], center[1] + size[1], center[2] + size[2] ]
    global.DrawBox( cornerA[0], cornerA[1], cornerA[2], cornerB[0], cornerB[1], cornerB[2], color[0], color[1], color[2], color[3] ?? 255 );
}

export function DrawRect( coords: Vector3, size: Vector3, color: Color ) {
    global.DrawRect( coords[0], coords[1], size[0], size[1], color[0], color[2], color[1], color[3] ?? 255 );
}

export function DrawText( pos: Vector2, content: string, textScale: number, color: Color ) {
    ClearDrawOrigin();
    AddText( pos, content, textScale, color );
}

export function AddText( pos: Vector2, content: string, textScale: number, color: Color ) {
    SetTextScale( 0, textScale );
    SetTextDropshadow( 1, 0, 0, 0, 0 );
    SetTextColour( color[0], color[1], color[2], color[3] ?? 255 );
    SetTextEntry( "STRING" );
    AddTextComponentString( content );
    global.DrawText( pos[0], pos[1] );
}

export function DrawText3D( worldPos: Vector3, content: string | string[], scale: number, color: Color = [255, 255, 255, 255], offset: Vector2 = [0, 0] ) {
    SetDrawOrigin( worldPos[0], worldPos[1], worldPos[2], 0 );
    if ( Array.isArray( content ) ) {
        for(const line of content) {
            AddText( offset, line, scale, color );
            offset[1] += scale;
        }
    } else {
        AddText( offset, content, scale, color );
    }
    ClearDrawOrigin();
}
        
export function DrawText3DEntity( entity: number, content: string, scale: number, color: Color = [255, 255, 255, 255], offset: Vector2 = [0, 0] ) {
    DrawText3D( GetEntityCoords(entity, true) as Vector3, content, scale, color, offset );
}


/// <summary>
/// Draws 3D text similar to <see cref="DrawText3D" /> but without the limitation of the amount of origins. Will shift around as it's slower than SetDrawOrigin
/// </summary>
/// <param name="worldPos"></param>
/// <param name="content"></param>
/// <param name="scale"></param>
/// <param name="color"></param>
/// <param name="offset"></param>
export function DrawText3D2( worldPos: Vector3, content: string, scale: number, color: Color = [255, 255, 255, 255], offset: Vector2 = [0, 0] )
{
    const screenPos = GetScreenCoords( worldPos ) ?? [0, 0]
    screenPos[0] += offset[0]
    screenPos[1] += offset[1]
    ClearDrawOrigin();
    AddText( screenPos, content, scale, color );
}

export function DrawText3D2Entity( entity: number, content: string, scale: number, color: Color, offset: Vector2 )
{
    DrawText3D2( GetEntityCoords(entity, true) as Vector3, content, scale, color, offset );
}
/// <summary>
/// Draws a marker, note that the Y position will be offset by +2f 
/// </summary>
export function DrawMarker( type: number, pos: Vector3, dir: Vector3, rot: Vector3, scale: Vector3, color: Color, bobUpAndDown: boolean, faceCamera: boolean, rotate: boolean, drawOnEnts: boolean, textureDict: string = "", textureName: string = "" )
{
    global.DrawMarker( type, pos[0], pos[1],
        pos[2] + 2, dir[0], dir[1], dir[2], rot[0], rot[1], rot[2], scale[0], scale[1], scale[2], color[0], color[1], color[2], color[3] ?? 255, bobUpAndDown, faceCamera, 2, rotate, textureDict,
        textureName,
        drawOnEnts );
}

export function GetEntitySize( entity: number ): Vector3 {
    const model = GetEntityModel( entity )
    const [min, max] = GetModelDimensions( model )
    return [max[0] - min[0], max[1] - min[1], max[2] - min[2]]
}


export function DrawConeMarker( entity: number, color: Color )
{
    const size = GetEntitySize( entity )
    const pos = GetEntityCoords( entity, true ) as Vector3
    pos[2] += size[2] / 2
    DrawMarker( 0, pos, [0,0,0], [0,0,0], [1,1,1], color, false, true, true, true, "", "" );
}

export function DrawEntityBox( entity: number, color: Color )
{
    const size = GetEntitySize( entity )
    const pos = GetEntityCoords( entity, true ) as Vector3
    const rot = GetEntityRotation(entity, 2) as Vector3
    pos[2] += size[2] / 2

    const boxSize = [ size[0] / 2, size[1] / 2, size[2] / 2 ] as Vector3
    DrawBoundingBoxFromCenter( pos, boxSize, rot, color );
}