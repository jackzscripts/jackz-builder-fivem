export type Vector3 = [number, number, number]
export type Vector2 = [number, number]
export type Color = [number, number, number, number?]

export const Colors: Record<string, Color> = {
    White: [255, 255, 255, 255],
    Black: [0, 0, 0, 255],
    Red: [255, 0, 0, 255],
    Blue: [0, 0, 255, 255],
    Green: [0, 255, 0, 255],
}