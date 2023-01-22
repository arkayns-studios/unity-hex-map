﻿using UnityEngine;

namespace Arkayns.Reckon.HM {

    public class HexGridChunk : MonoBehaviour {

        // -- Variables --
        public HexMesh terrain, rivers, roads;
        private HexCell[] cells;
        private Canvas gridCanvas;

        // -- Built-In Methods --
        private void Awake() {
            gridCanvas = GetComponentInChildren<Canvas>();
            cells = new HexCell[HexMetrics.ChunkSizeX * HexMetrics.ChunkSizeZ];
            ShowUI(false);
        } // Awake ()
        
        private void LateUpdate() {
            Triangulate();
            enabled = false;
        } // LateUpdate ()

        // -- Methods --
        public void AddCell(int index, HexCell cell) {
            cells[index] = cell;
            cell.chunk = this;
            cell.transform.SetParent(transform, false);
            cell.uiRect.SetParent(gridCanvas.transform, false);
        } // AddCell ()

        public void Refresh() {
            enabled = true;
        } // Refresh ()

        public void ShowUI(bool visible) {
            gridCanvas.gameObject.SetActive(visible);
        } // ShowUI ()
        
        // -- Triangulate Methods --
        public void Triangulate() {
            terrain.Clear();
            rivers.Clear();
            roads.Clear();
            foreach (var t in cells) Triangulate(t);
            terrain.Apply();
            rivers.Apply();
            roads.Apply();
        } // Triangulate ()

        private void Triangulate(HexCell cell) {
            for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++) {
                Triangulate(d, cell);
            }
        } // Triangulate ()

        private void Triangulate(HexDirection direction, HexCell cell) {
            var center = cell.Position;
            var e = new EdgeVertices(center + HexMetrics.GetFirstSolidCorner(direction), center + HexMetrics.GetSecondSolidCorner(direction));

            if (cell.HasRiver) {
                if (cell.HasRiverThroughEdge(direction)) {
                    e.v3.y = cell.StreamBedY;
                    if (cell.HasRiverBeginOrEnd) {
                        TriangulateWithRiverBeginOrEnd(direction, cell, center, e);
                    } else {
                        TriangulateWithRiver(direction, cell, center, e);
                    }
                } else {
                    TriangulateAdjacentToRiver(direction, cell, center, e);
                }
            } else {
                TriangulateWithoutRiver(direction, cell, center, e);
            }

            if (direction <= HexDirection.SE) TriangulateConnection(direction, cell, e);
        } // Triangulate ()

        private void TriangulateAdjacentToRiver(HexDirection direction, HexCell cell, Vector3 center, EdgeVertices e) {
            if (cell.HasRiverThroughEdge(direction.Next())) {
                if (cell.HasRiverThroughEdge(direction.Previous())) {
                    center += HexMetrics.GetSolidEdgeMiddle(direction) *
                              (HexMetrics.InnerToOuter * 0.5f);
                }
                else if (
                    cell.HasRiverThroughEdge(direction.Previous2())
                ) {
                    center += HexMetrics.GetFirstSolidCorner(direction) * 0.25f;
                }
            }
            else if (
                cell.HasRiverThroughEdge(direction.Previous()) &&
                cell.HasRiverThroughEdge(direction.Next2())
            ) {
                center += HexMetrics.GetSecondSolidCorner(direction) * 0.25f;
            }

            EdgeVertices m = new EdgeVertices(
                Vector3.Lerp(center, e.v1, 0.5f),
                Vector3.Lerp(center, e.v5, 0.5f)
            );

            TriangulateEdgeStrip(m, cell.Color, e, cell.Color);
            TriangulateEdgeFan(center, m, cell.Color);
        } // TriangulateAdjacentToRiver ()

        private void TriangulateWithRiverBeginOrEnd(HexDirection direction, HexCell cell, Vector3 center, EdgeVertices e) {
            EdgeVertices m = new EdgeVertices(
                Vector3.Lerp(center, e.v1, 0.5f),
                Vector3.Lerp(center, e.v5, 0.5f)
            );
            m.v3.y = e.v3.y;

            TriangulateEdgeStrip(m, cell.Color, e, cell.Color);
            TriangulateEdgeFan(center, m, cell.Color);

            bool reversed = cell.HasIncomingRiver;
            TriangulateRiverQuad(
                m.v2, m.v4, e.v2, e.v4, cell.RiverSurfaceY, 0.6f, reversed
            );
            center.y = m.v2.y = m.v4.y = cell.RiverSurfaceY;
            rivers.AddTriangle(center, m.v2, m.v4);
            if (reversed) {
                rivers.AddTriangleUV(
                    new Vector2(0.5f, 0.4f),
                    new Vector2(1f, 0.2f), new Vector2(0f, 0.2f)
                );
            }
            else {
                rivers.AddTriangleUV(
                    new Vector2(0.5f, 0.4f),
                    new Vector2(0f, 0.6f), new Vector2(1f, 0.6f)
                );
            }
        } // TriangulateWithRiverBeginOrEnd ()

        private void TriangulateWithRiver(HexDirection direction, HexCell cell, Vector3 center, EdgeVertices e) {
            Vector3 centerL, centerR;
            if (cell.HasRiverThroughEdge(direction.Opposite())) {
                centerL = center +
                          HexMetrics.GetFirstSolidCorner(direction.Previous()) * 0.25f;
                centerR = center +
                          HexMetrics.GetSecondSolidCorner(direction.Next()) * 0.25f;
            }
            else if (cell.HasRiverThroughEdge(direction.Next())) {
                centerL = center;
                centerR = Vector3.Lerp(center, e.v5, 2f / 3f);
            }
            else if (cell.HasRiverThroughEdge(direction.Previous())) {
                centerL = Vector3.Lerp(center, e.v1, 2f / 3f);
                centerR = center;
            }
            else if (cell.HasRiverThroughEdge(direction.Next2())) {
                centerL = center;
                centerR = center +
                          HexMetrics.GetSolidEdgeMiddle(direction.Next()) *
                          (0.5f * HexMetrics.InnerToOuter);
            }
            else {
                centerL = center +
                          HexMetrics.GetSolidEdgeMiddle(direction.Previous()) *
                          (0.5f * HexMetrics.InnerToOuter);
                centerR = center;
            }

            center = Vector3.Lerp(centerL, centerR, 0.5f);

            EdgeVertices m = new EdgeVertices(
                Vector3.Lerp(centerL, e.v1, 0.5f),
                Vector3.Lerp(centerR, e.v5, 0.5f),
                1f / 6f
            );
            m.v3.y = center.y = e.v3.y;

            TriangulateEdgeStrip(m, cell.Color, e, cell.Color);

            terrain.AddTriangle(centerL, m.v1, m.v2);
            terrain.AddTriangleColor(cell.Color);
            terrain.AddQuad(centerL, center, m.v2, m.v3);
            terrain.AddQuadColor(cell.Color);
            terrain.AddQuad(center, centerR, m.v3, m.v4);
            terrain.AddQuadColor(cell.Color);
            terrain.AddTriangle(centerR, m.v4, m.v5);
            terrain.AddTriangleColor(cell.Color);

            bool reversed = cell.IncomingRiver == direction;
            TriangulateRiverQuad(
                centerL, centerR, m.v2, m.v4, cell.RiverSurfaceY, 0.4f, reversed
            );
            TriangulateRiverQuad(
                m.v2, m.v4, e.v2, e.v4, cell.RiverSurfaceY, 0.6f, reversed
            );
        } // TriangulateWithRiver ()

        private void TriangulateWithoutRiver (HexDirection direction, HexCell cell, Vector3 center, EdgeVertices e) {
            TriangulateEdgeFan(center, e, cell.Color);
            if (cell.HasRoadThroughEdge(direction)) 
                TriangulateRoad(center, Vector3.Lerp(center, e.v1, 0.5f), Vector3.Lerp(center, e.v5, 0.5f), e);
        } // TriangulateWithoutRiver ()
        
        private void TriangulateRoadSegment (Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Vector3 v5, Vector3 v6) {
            roads.AddQuad(v1, v2, v4, v5);
            roads.AddQuad(v2, v3, v5, v6);
            roads.AddQuadUV(0f, 1f, 0f, 0f);
            roads.AddQuadUV(1f, 0f, 0f, 0f);
        } // TriangulateRoadSegment ()
        
        private void TriangulateRoad (Vector3 center, Vector3 mL, Vector3 mR, EdgeVertices e) {
            var mC = Vector3.Lerp(mL, mR, 0.5f);
            TriangulateRoadSegment(mL, mC, mR, e.v2, e.v3, e.v4);
            roads.AddTriangle(center, mL, mC);
            roads.AddTriangle(center, mC, mR);
            roads.AddTriangleUV(new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(1f, 0f));
            roads.AddTriangleUV(new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(0f, 0f));
        } // TriangulateRoad ()
        
        private void TriangulateConnection(HexDirection direction, HexCell cell, EdgeVertices e1) {
            var neighbor = cell.GetNeighbor(direction);
            if (neighbor == null) return;

            var bridge = HexMetrics.GetBridge(direction);
            bridge.y = neighbor.Position.y - cell.Position.y;
            var e2 = new EdgeVertices(e1.v1 + bridge, e1.v5 + bridge);

            if (cell.HasRiverThroughEdge(direction)) {
                e2.v3.y = neighbor.StreamBedY;
                TriangulateRiverQuad(
                    e1.v2, e1.v4, e2.v2, e2.v4,
                    cell.RiverSurfaceY, neighbor.RiverSurfaceY, 0.8f,
                    cell.HasIncomingRiver && cell.IncomingRiver == direction
                );
            }

            if (cell.GetEdgeType(direction) == HexEdgeType.Slope) {
                TriangulateEdgeTerraces(e1, cell, e2, neighbor, cell.HasRoadThroughEdge(direction));
            } else {
                TriangulateEdgeStrip(e1, cell.Color, e2, neighbor.Color, cell.HasRoadThroughEdge(direction));
            }

            var nextNeighbor = cell.GetNeighbor(direction.Next());
            if (direction <= HexDirection.E && nextNeighbor != null) {
                var v5 = e1.v5 + HexMetrics.GetBridge(direction.Next());
                v5.y = nextNeighbor.Position.y;

                if (cell.Elevation <= neighbor.Elevation) {
                    if (cell.Elevation <= nextNeighbor.Elevation) {
                        TriangulateCorner(e1.v5, cell, e2.v5, neighbor, v5, nextNeighbor);
                    } else {
                        TriangulateCorner(v5, nextNeighbor, e1.v5, cell, e2.v5, neighbor);
                    }
                } else if (neighbor.Elevation <= nextNeighbor.Elevation) {
                    TriangulateCorner(e2.v5, neighbor, v5, nextNeighbor, e1.v5, cell);
                } else {
                    TriangulateCorner(v5, nextNeighbor, e1.v5, cell, e2.v5, neighbor);
                }
            }
        } // TriangulateConnection ()

        private void TriangulateCorner(Vector3 bottom, HexCell bottomCell, Vector3 left, HexCell leftCell, Vector3 right, HexCell rightCell) {
            HexEdgeType leftEdgeType = bottomCell.GetEdgeType(leftCell);
            HexEdgeType rightEdgeType = bottomCell.GetEdgeType(rightCell);

            if (leftEdgeType == HexEdgeType.Slope) {
                if (rightEdgeType == HexEdgeType.Slope) {
                    TriangulateCornerTerraces(
                        bottom, bottomCell, left, leftCell, right, rightCell
                    );
                }
                else if (rightEdgeType == HexEdgeType.Flat) {
                    TriangulateCornerTerraces(
                        left, leftCell, right, rightCell, bottom, bottomCell
                    );
                }
                else {
                    TriangulateCornerTerracesCliff(
                        bottom, bottomCell, left, leftCell, right, rightCell
                    );
                }
            }
            else if (rightEdgeType == HexEdgeType.Slope) {
                if (leftEdgeType == HexEdgeType.Flat) {
                    TriangulateCornerTerraces(
                        right, rightCell, bottom, bottomCell, left, leftCell
                    );
                }
                else {
                    TriangulateCornerCliffTerraces(
                        bottom, bottomCell, left, leftCell, right, rightCell
                    );
                }
            }
            else if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope) {
                if (leftCell.Elevation < rightCell.Elevation) {
                    TriangulateCornerCliffTerraces(
                        right, rightCell, bottom, bottomCell, left, leftCell
                    );
                }
                else {
                    TriangulateCornerTerracesCliff(
                        left, leftCell, right, rightCell, bottom, bottomCell
                    );
                }
            }
            else {
                terrain.AddTriangle(bottom, left, right);
                terrain.AddTriangleColor(
                    bottomCell.Color, leftCell.Color, rightCell.Color
                );
            }
        } // TriangulateCorner ()

        private void TriangulateEdgeTerraces(EdgeVertices begin, HexCell beginCell, EdgeVertices end, HexCell endCell, bool hasRoad) {
            var e2 = EdgeVertices.TerraceLerp(begin, end, 1);
            var c2 = HexMetrics.TerraceLerp(beginCell.Color, endCell.Color, 1);

            TriangulateEdgeStrip(begin, beginCell.Color, e2, c2, hasRoad);

            for (var i = 2; i < HexMetrics.TerraceSteps; i++) {
                var e1 = e2;
                var c1 = c2;
                e2 = EdgeVertices.TerraceLerp(begin, end, i);
                c2 = HexMetrics.TerraceLerp(beginCell.Color, endCell.Color, i);
                TriangulateEdgeStrip(e1, c1, e2, c2, hasRoad);
            }

            TriangulateEdgeStrip(e2, c2, end, endCell.Color, hasRoad);
        } // TriangulateEdgeTerraces ()

        private void TriangulateCornerTerraces(Vector3 begin, HexCell beginCell, Vector3 left, HexCell leftCell, Vector3 right, HexCell rightCell) {
            Vector3 v3 = HexMetrics.TerraceLerp(begin, left, 1);
            Vector3 v4 = HexMetrics.TerraceLerp(begin, right, 1);
            Color c3 = HexMetrics.TerraceLerp(beginCell.Color, leftCell.Color, 1);
            Color c4 = HexMetrics.TerraceLerp(beginCell.Color, rightCell.Color, 1);

            terrain.AddTriangle(begin, v3, v4);
            terrain.AddTriangleColor(beginCell.Color, c3, c4);

            for (int i = 2; i < HexMetrics.TerraceSteps; i++) {
                Vector3 v1 = v3;
                Vector3 v2 = v4;
                Color c1 = c3;
                Color c2 = c4;
                v3 = HexMetrics.TerraceLerp(begin, left, i);
                v4 = HexMetrics.TerraceLerp(begin, right, i);
                c3 = HexMetrics.TerraceLerp(beginCell.Color, leftCell.Color, i);
                c4 = HexMetrics.TerraceLerp(beginCell.Color, rightCell.Color, i);
                terrain.AddQuad(v1, v2, v3, v4);
                terrain.AddQuadColor(c1, c2, c3, c4);
            }

            terrain.AddQuad(v3, v4, left, right);
            terrain.AddQuadColor(c3, c4, leftCell.Color, rightCell.Color);
        } // TriangulateCornerTerraces ()

        private void TriangulateCornerTerracesCliff(Vector3 begin, HexCell beginCell, Vector3 left, HexCell leftCell, Vector3 right, HexCell rightCell) {
            float b = 1f / (rightCell.Elevation - beginCell.Elevation);
            if (b < 0) {
                b = -b;
            }

            Vector3 boundary = Vector3.Lerp(
                HexMetrics.Perturb(begin), HexMetrics.Perturb(right), b
            );
            Color boundaryColor = Color.Lerp(beginCell.Color, rightCell.Color, b);

            TriangulateBoundaryTriangle(
                begin, beginCell, left, leftCell, boundary, boundaryColor
            );

            if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope) {
                TriangulateBoundaryTriangle(
                    left, leftCell, right, rightCell, boundary, boundaryColor
                );
            }
            else {
                terrain.AddTriangleUnperturbed(
                    HexMetrics.Perturb(left), HexMetrics.Perturb(right), boundary
                );
                terrain.AddTriangleColor(
                    leftCell.Color, rightCell.Color, boundaryColor
                );
            }
        } // TriangulateCornerTerracesCliff ()

        private void TriangulateCornerCliffTerraces(Vector3 begin, HexCell beginCell, Vector3 left, HexCell leftCell, Vector3 right, HexCell rightCell) {
            float b = 1f / (leftCell.Elevation - beginCell.Elevation);
            if (b < 0) {
                b = -b;
            }

            Vector3 boundary = Vector3.Lerp(
                HexMetrics.Perturb(begin), HexMetrics.Perturb(left), b
            );
            Color boundaryColor = Color.Lerp(beginCell.Color, leftCell.Color, b);

            TriangulateBoundaryTriangle(
                right, rightCell, begin, beginCell, boundary, boundaryColor
            );

            if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope) {
                TriangulateBoundaryTriangle(
                    left, leftCell, right, rightCell, boundary, boundaryColor
                );
            }
            else {
                terrain.AddTriangleUnperturbed(
                    HexMetrics.Perturb(left), HexMetrics.Perturb(right), boundary
                );
                terrain.AddTriangleColor(
                    leftCell.Color, rightCell.Color, boundaryColor
                );
            }
        } // TriangulateCornerCliffTerraces ()

        private void TriangulateBoundaryTriangle(Vector3 begin, HexCell beginCell, Vector3 left, HexCell leftCell, Vector3 boundary, Color boundaryColor) {
            Vector3 v2 = HexMetrics.Perturb(HexMetrics.TerraceLerp(begin, left, 1));
            Color c2 = HexMetrics.TerraceLerp(beginCell.Color, leftCell.Color, 1);

            terrain.AddTriangleUnperturbed(HexMetrics.Perturb(begin), v2, boundary);
            terrain.AddTriangleColor(beginCell.Color, c2, boundaryColor);

            for (int i = 2; i < HexMetrics.TerraceSteps; i++) {
                Vector3 v1 = v2;
                Color c1 = c2;
                v2 = HexMetrics.Perturb(HexMetrics.TerraceLerp(begin, left, i));
                c2 = HexMetrics.TerraceLerp(beginCell.Color, leftCell.Color, i);
                terrain.AddTriangleUnperturbed(v1, v2, boundary);
                terrain.AddTriangleColor(c1, c2, boundaryColor);
            }

            terrain.AddTriangleUnperturbed(v2, HexMetrics.Perturb(left), boundary);
            terrain.AddTriangleColor(c2, leftCell.Color, boundaryColor);
        } // TriangulateBoundaryTriangle ()

        private void TriangulateEdgeFan(Vector3 center, EdgeVertices edge, Color color) {
            terrain.AddTriangle(center, edge.v1, edge.v2);
            terrain.AddTriangleColor(color);
            terrain.AddTriangle(center, edge.v2, edge.v3);
            terrain.AddTriangleColor(color);
            terrain.AddTriangle(center, edge.v3, edge.v4);
            terrain.AddTriangleColor(color);
            terrain.AddTriangle(center, edge.v4, edge.v5);
            terrain.AddTriangleColor(color);
        } // TriangulateEdgeFan ()

        private void TriangulateEdgeStrip(EdgeVertices e1, Color c1, EdgeVertices e2, Color c2, bool hasRoad = false) {
            terrain.AddQuad(e1.v1, e1.v2, e2.v1, e2.v2);
            terrain.AddQuadColor(c1, c2);
            terrain.AddQuad(e1.v2, e1.v3, e2.v2, e2.v3);
            terrain.AddQuadColor(c1, c2);
            terrain.AddQuad(e1.v3, e1.v4, e2.v3, e2.v4);
            terrain.AddQuadColor(c1, c2);
            terrain.AddQuad(e1.v4, e1.v5, e2.v4, e2.v5);
            terrain.AddQuadColor(c1, c2);
            
            if (hasRoad) TriangulateRoadSegment(e1.v2, e1.v3, e1.v4, e2.v2, e2.v3, e2.v4);
        } // TriangulateEdgeStrip ()

        private void TriangulateRiverQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, float y, float v, bool reversed) {
            TriangulateRiverQuad(v1, v2, v3, v4, y, y, v, reversed);
        } // TriangulateRiverQuad ()

        private void TriangulateRiverQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, float y1, float y2, float v, bool reversed) {
            v1.y = v2.y = y1;
            v3.y = v4.y = y2;
            rivers.AddQuad(v1, v2, v3, v4);
            if (reversed) {
                rivers.AddQuadUV(1f, 0f, 0.8f - v, 0.6f - v);
            }
            else {
                rivers.AddQuadUV(0f, 1f, v, v + 0.2f);
            }
        } // TriangulateRiverQuad ()

    } // Class HexGridChunk

} // Namespace Arkayns Reckon HexMap