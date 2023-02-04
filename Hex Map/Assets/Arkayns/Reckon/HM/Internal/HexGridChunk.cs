using UnityEngine;

namespace Arkayns.Reckon.HM {

    public class HexGridChunk : MonoBehaviour {

        // -- Variables --
        public HexMesh terrain, rivers, roads, water, waterShore, estuaries;
        public HexFeatureManager features;
        private HexCell[] cells;
        private Canvas gridCanvas;
        private static Color m_color1 = new Color(1f, 0f, 0f);
        private static Color m_color2 = new Color(0f, 1f, 0f);
        private static Color m_color3 = new Color(0f, 0f, 1f);
        
        // -- Built-In Methods --
        private void Awake() {
            gridCanvas = GetComponentInChildren<Canvas>();
            cells = new HexCell[HexMetrics.ChunkSizeX * HexMetrics.ChunkSizeZ];
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

        private Vector2 GetRoadInterpolators (HexDirection direction, HexCell cell) {
            Vector2 interpolators;
            if (cell.HasRoadThroughEdge(direction)) {
                interpolators.x = interpolators.y = 0.5f;
            } else {
                interpolators.x = cell.HasRoadThroughEdge(direction.Previous()) ? 0.5f : 0.25f;
                interpolators.y = cell.HasRoadThroughEdge(direction.Next()) ? 0.5f : 0.25f;
            }
            return interpolators;
        } // GetRoadInterpolators ()
        
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
            water.Clear();
            waterShore.Clear();
            estuaries.Clear();
            features.Clear();
            foreach (var t in cells) Triangulate(t);
            terrain.Apply();
            rivers.Apply();
            roads.Apply();
            water.Apply();
            waterShore.Apply();
            estuaries.Apply();
            features.Apply();
        } // Triangulate ()

        private void Triangulate(HexCell cell) {
            for (var d = HexDirection.NE; d <= HexDirection.NW; d++) Triangulate(d, cell);
            if (!cell.IsUnderwater) {
                if (!cell.HasRiver && !cell.HasRoads) features.AddFeature(cell, cell.Position);
                if (cell.IsSpecial) features.AddSpecialFeature(cell, cell.Position);
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
                if (!cell.IsUnderwater && !cell.HasRoadThroughEdge(direction)) {
                    features.AddFeature(cell, (center + e.v1 + e.v5) * (1f / 3f));
                }
            }

            if (direction <= HexDirection.SE) TriangulateConnection(direction, cell, e);
            if (cell.IsUnderwater) TriangulateWater(direction, cell, center);
        } // Triangulate ()

        private void TriangulateWithoutRiver (HexDirection direction, HexCell cell, Vector3 center, EdgeVertices e) {
            TriangulateEdgeFan(center, e, cell.TerrainTypeIndex);
            if (cell.HasRoads) {
                var interpolators = GetRoadInterpolators(direction, cell);
                TriangulateRoad(center, 
                    Vector3.Lerp(center, e.v1, interpolators.x), 
                    Vector3.Lerp(center, e.v5, interpolators.y), 
                    e, cell.HasRoadThroughEdge(direction));
            }
        } // TriangulateWithoutRiver ()

        private void TriangulateWater (HexDirection direction, HexCell cell, Vector3 center) {
            center.y = cell.WaterSurfaceY;
            HexCell neighbor = cell.GetNeighbor(direction);
            if (neighbor != null && !neighbor.IsUnderwater) {
                TriangulateWaterShore(direction, cell, neighbor, center);
            } else {
                TriangulateOpenWater(direction, cell, neighbor, center);
            }
        } // TriangulateWater ()

        private void TriangulateOpenWater(HexDirection direction, HexCell cell, HexCell neighbor, Vector3 center) {
            var c1 = center + HexMetrics.GetFirstWaterCorner(direction);
            var c2 = center + HexMetrics.GetSecondWaterCorner(direction);

            water.AddTriangle(center, c1, c2);
            
            if (direction <= HexDirection.SE) {
                var bridge = HexMetrics.GetWaterBridge(direction);
                var e1 = c1 + bridge;
                var e2 = c2 + bridge;

                water.AddQuad(c1, c2, e1, e2);
                
                if (direction <= HexDirection.E) {
                    var nextNeighbor = cell.GetNeighbor(direction.Next());
                    if (nextNeighbor == null || !nextNeighbor.IsUnderwater) return;
                    water.AddTriangle(c2, e2, c2 + HexMetrics.GetWaterBridge(direction.Next()));
                }
            }
        } // TriangulateOpenWater ()
        
        private void TriangulateWaterShore (HexDirection direction, HexCell cell, HexCell neighbor, Vector3 center) {
            var e1 = new EdgeVertices(center + HexMetrics.GetFirstWaterCorner(direction), center + HexMetrics.GetSecondWaterCorner(direction));
            water.AddTriangle(center, e1.v1, e1.v2);
            water.AddTriangle(center, e1.v2, e1.v3);
            water.AddTriangle(center, e1.v3, e1.v4);
            water.AddTriangle(center, e1.v4, e1.v5);
            
            var center2 = neighbor.Position;
            center2.y = center.y;
            var e2 = new EdgeVertices(center2  + HexMetrics.GetSecondSolidCorner(direction.Opposite()), center2  + HexMetrics.GetFirstSolidCorner(direction.Opposite()));
            
            if (cell.HasRiverThroughEdge(direction)) {
                TriangulateEstuary(e1, e2, cell.IncomingRiver == direction);
            } else {
                waterShore.AddQuad(e1.v1, e1.v2, e2.v1, e2.v2);
                waterShore.AddQuad(e1.v2, e1.v3, e2.v2, e2.v3);
                waterShore.AddQuad(e1.v3, e1.v4, e2.v3, e2.v4);
                waterShore.AddQuad(e1.v4, e1.v5, e2.v4, e2.v5);
                waterShore.AddQuadUV(0f, 0f, 0f, 1f);
                waterShore.AddQuadUV(0f, 0f, 0f, 1f);
                waterShore.AddQuadUV(0f, 0f, 0f, 1f);
                waterShore.AddQuadUV(0f, 0f, 0f, 1f);
            }

            var nextNeighbor = cell.GetNeighbor(direction.Next());
            if (nextNeighbor != null) {
                var v3 = nextNeighbor.Position + (nextNeighbor.IsUnderwater ? HexMetrics.GetFirstWaterCorner(direction.Previous()) : HexMetrics.GetFirstSolidCorner(direction.Previous()));
                v3.y = center.y;
                waterShore.AddTriangle(e1.v5, e2.v5, v3);
                waterShore.AddTriangleUV(new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, nextNeighbor.IsUnderwater ? 0f : 1f));
            }
        } // Class TriangulateWaterShore
        
        private void TriangulateEstuary (EdgeVertices e1, EdgeVertices e2, bool incomingRiver) {
            waterShore.AddTriangle(e2.v1, e1.v2, e1.v1);
            waterShore.AddTriangle(e2.v5, e1.v5, e1.v4);
            waterShore.AddTriangleUV(new Vector2(0f, 1f), new Vector2(0f, 0f), new Vector2(0f, 0f));
            waterShore.AddTriangleUV(new Vector2(0f, 1f), new Vector2(0f, 0f), new Vector2(0f, 0f));
            
            estuaries.AddQuad(e2.v1, e1.v2, e2.v2, e1.v3);
            estuaries.AddTriangle(e1.v3, e2.v2, e2.v4);
            estuaries.AddQuad(e1.v3, e1.v4, e2.v4, e2.v5);
            
            estuaries.AddQuadUV(new Vector2(0f, 1f), new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0f, 0f));
            estuaries.AddTriangleUV(new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(1f, 1f));
            estuaries.AddQuadUV(new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0f, 1f));

            if (incomingRiver) {
                estuaries.AddQuadUV2(new Vector2(1.5f, 1f), new Vector2(0.7f, 1.15f), new Vector2(1f, 0.8f), new Vector2(0.5f, 1.1f));
                estuaries.AddTriangleUV2(new Vector2(0.5f, 1.1f), new Vector2(1f, 0.8f), new Vector2(0f, 0.8f));
                estuaries.AddQuadUV2(new Vector2(0.5f, 1.1f), new Vector2(0.3f, 1.15f), new Vector2(0f, 0.8f), new Vector2(-0.5f, 1f));
            } else {
                estuaries.AddQuadUV2(new Vector2(-0.5f, -0.2f), new Vector2(0.3f, -0.35f), new Vector2(0f, 0f), new Vector2(0.5f, -0.3f));
                estuaries.AddTriangleUV2(new Vector2(0.5f, -0.3f), new Vector2(0f, 0f), new Vector2(1f, 0f));
                estuaries.AddQuadUV2(new Vector2(0.5f, -0.3f), new Vector2(0.7f, -0.35f), new Vector2(1f, 0f), new Vector2(1.5f, -0.2f));
            }
        } // TriangulateEstuary ()
        
        private void TriangulateWaterfallInWater (Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, float y1, float y2, float waterY) {
            v1.y = v2.y = y1;
            v3.y = v4.y = y2;
            v1 = HexMetrics.Perturb(v1);
            v2 = HexMetrics.Perturb(v2);
            v3 = HexMetrics.Perturb(v3);
            v4 = HexMetrics.Perturb(v4);
            var t = (waterY - y2) / (y1 - y2);
            v3 = Vector3.Lerp(v3, v1, t);
            v4 = Vector3.Lerp(v4, v2, t);
            rivers.AddQuadUnperturbed(v1, v2, v3, v4);
            rivers.AddQuadUV(0f, 1f, 0.8f, 1f);
        } // TriangulateWaterfallInWater ()
        
        private void TriangulateAdjacentToRiver(HexDirection direction, HexCell cell, Vector3 center, EdgeVertices e) {
            if (cell.HasRoads) TriangulateRoadAdjacentToRiver(direction, cell, center, e);
            
            if (cell.HasRiverThroughEdge(direction.Next())) {
                if (cell.HasRiverThroughEdge(direction.Previous())) {
                    center += HexMetrics.GetSolidEdgeMiddle(direction) * (HexMetrics.InnerToOuter * 0.5f);
                } else if (cell.HasRiverThroughEdge(direction.Previous2())) {
                    center += HexMetrics.GetFirstSolidCorner(direction) * 0.25f;
                }
            }
            else if (cell.HasRiverThroughEdge(direction.Previous()) && cell.HasRiverThroughEdge(direction.Next2())) {
                center += HexMetrics.GetSecondSolidCorner(direction) * 0.25f;
            }

            var m = new EdgeVertices(Vector3.Lerp(center, e.v1, 0.5f), Vector3.Lerp(center, e.v5, 0.5f));

            TriangulateEdgeStrip(m, m_color1, cell.TerrainTypeIndex, e, m_color1, cell.TerrainTypeIndex);
            TriangulateEdgeFan(center, m, cell.TerrainTypeIndex);
            
            if (!cell.IsUnderwater && !cell.HasRoadThroughEdge(direction)) {
                features.AddFeature(cell, (center + e.v1 + e.v5) * (1f / 3f));
            }
        } // TriangulateAdjacentToRiver ()

        private void TriangulateWithRiverBeginOrEnd(HexDirection direction, HexCell cell, Vector3 center, EdgeVertices e) {
            var m = new EdgeVertices(Vector3.Lerp(center, e.v1, 0.5f), Vector3.Lerp(center, e.v5, 0.5f));
            m.v3.y = e.v3.y;

            TriangulateEdgeStrip(m, m_color1, cell.TerrainTypeIndex, e, m_color1, cell.TerrainTypeIndex);
            TriangulateEdgeFan(center, m, cell.TerrainTypeIndex);

            if (!cell.IsUnderwater) {
                var reversed = cell.HasIncomingRiver;
                TriangulateRiverQuad(m.v2, m.v4, e.v2, e.v4, cell.RiverSurfaceY, 0.6f, reversed);
                center.y = m.v2.y = m.v4.y = cell.RiverSurfaceY;
                rivers.AddTriangle(center, m.v2, m.v4);
                if (reversed) {
                    rivers.AddTriangleUV(new Vector2(0.5f, 0.4f), new Vector2(1f, 0.2f), new Vector2(0f, 0.2f));
                } else {
                    rivers.AddTriangleUV(new Vector2(0.5f, 0.4f), new Vector2(0f, 0.6f), new Vector2(1f, 0.6f));
                }
            }
        } // TriangulateWithRiverBeginOrEnd ()

        private void TriangulateWithRiver(HexDirection direction, HexCell cell, Vector3 center, EdgeVertices e) {
            Vector3 centerL, centerR;
            if (cell.HasRiverThroughEdge(direction.Opposite())) {
                centerL = center + HexMetrics.GetFirstSolidCorner(direction.Previous()) * 0.25f;
                centerR = center + HexMetrics.GetSecondSolidCorner(direction.Next()) * 0.25f;
            } else if (cell.HasRiverThroughEdge(direction.Next())) {
                centerL = center;
                centerR = Vector3.Lerp(center, e.v5, 2f / 3f);
            } else if (cell.HasRiverThroughEdge(direction.Previous())) {
                centerL = Vector3.Lerp(center, e.v1, 2f / 3f);
                centerR = center;
            } else if (cell.HasRiverThroughEdge(direction.Next2())) {
                centerL = center;
                centerR = center + HexMetrics.GetSolidEdgeMiddle(direction.Next()) * (0.5f * HexMetrics.InnerToOuter);
            } else {
                centerL = center + HexMetrics.GetSolidEdgeMiddle(direction.Previous()) * (0.5f * HexMetrics.InnerToOuter);
                centerR = center;
            }

            center = Vector3.Lerp(centerL, centerR, 0.5f);

            var m = new EdgeVertices(Vector3.Lerp(centerL, e.v1, 0.5f), Vector3.Lerp(centerR, e.v5, 0.5f), 1f / 6f);
            m.v3.y = center.y = e.v3.y;

            TriangulateEdgeStrip(m, m_color1, cell.TerrainTypeIndex, e, m_color1, cell.TerrainTypeIndex);

            terrain.AddTriangle(centerL, m.v1, m.v2);
            terrain.AddQuad(centerL, center, m.v2, m.v3);
            terrain.AddQuad(center, centerR, m.v3, m.v4);
            terrain.AddTriangle(centerR, m.v4, m.v5);

            terrain.AddTriangleColor(m_color1);
            terrain.AddQuadColor(m_color1);
            terrain.AddQuadColor(m_color1);
            terrain.AddTriangleColor(m_color1);
            
            Vector3 types;
            types.x = types.y = types.z = cell.TerrainTypeIndex;
            terrain.AddTriangleTerrainTypes(types);
            terrain.AddQuadTerrainTypes(types);
            terrain.AddQuadTerrainTypes(types);
            terrain.AddTriangleTerrainTypes(types);
            
            if (!cell.IsUnderwater) {
                var reversed = cell.IncomingRiver == direction;
                TriangulateRiverQuad(centerL, centerR, m.v2, m.v4, cell.RiverSurfaceY, 0.4f, reversed);
                TriangulateRiverQuad(m.v2, m.v4, e.v2, e.v4, cell.RiverSurfaceY, 0.6f, reversed);
            }
        } // TriangulateWithRiver ()
        
        private void TriangulateRoad (Vector3 center, Vector3 mL, Vector3 mR, EdgeVertices e, bool hasRoadThroughCellEdge) {
            if (hasRoadThroughCellEdge) {
                var mC = Vector3.Lerp(mL, mR, 0.5f);
                TriangulateRoadSegment(mL, mC, mR, e.v2, e.v3, e.v4);
                roads.AddTriangle(center, mL, mC);
                roads.AddTriangle(center, mC, mR);
                roads.AddTriangleUV(new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(1f, 0f));
                roads.AddTriangleUV(new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(0f, 0f));
            } else TriangulateRoadEdge(center, mL, mR);
        } // TriangulateRoad ()
        
        private void TriangulateRoadSegment (Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Vector3 v5, Vector3 v6) {
            roads.AddQuad(v1, v2, v4, v5);
            roads.AddQuad(v2, v3, v5, v6);
            roads.AddQuadUV(0f, 1f, 0f, 0f);
            roads.AddQuadUV(1f, 0f, 0f, 0f);
        } // TriangulateRoadSegment ()
        
        private void TriangulateRoadAdjacentToRiver (HexDirection direction, HexCell cell, Vector3 center, EdgeVertices e) {
            var hasRoadThroughEdge = cell.HasRoadThroughEdge(direction);
            var previousHasRiver = cell.HasRiverThroughEdge(direction.Previous());
            var nextHasRiver = cell.HasRiverThroughEdge(direction.Next());
            var interpolators = GetRoadInterpolators(direction, cell);
            
            var roadCenter = center;
            if (cell.HasRiverBeginOrEnd) {
                roadCenter += HexMetrics.GetSolidEdgeMiddle(cell.RiverBeginOrEndDirection.Opposite()) * (1f / 3f);
            } else if (cell.IncomingRiver == cell.OutgoingRiver.Opposite()) {
                Vector3 corner;
                if (previousHasRiver) {
                    if (!hasRoadThroughEdge && !cell.HasRoadThroughEdge(direction.Next())) return;
                    corner = HexMetrics.GetSecondSolidCorner(direction);
                } else {
                    if (!hasRoadThroughEdge && !cell.HasRoadThroughEdge(direction.Previous())) return;
                    corner = HexMetrics.GetFirstSolidCorner(direction);
                }
                roadCenter += corner * 0.5f;
                if (cell.IncomingRiver == direction.Next() && (cell.HasRoadThroughEdge(direction.Next2()) || cell.HasRoadThroughEdge(direction.Opposite())))
                    features.AddBridge(roadCenter, center - corner * 0.5f);
                center += corner * 0.25f;
            } else if (cell.IncomingRiver == cell.OutgoingRiver.Previous()) {
                roadCenter -= HexMetrics.GetSecondCorner(cell.IncomingRiver) * 0.2f;
            } else if (cell.IncomingRiver == cell.OutgoingRiver.Next()) {
                roadCenter -= HexMetrics.GetFirstCorner(cell.IncomingRiver) * 0.2f;
            } else if (previousHasRiver && nextHasRiver) {
                if (!hasRoadThroughEdge) return;
                var offset = HexMetrics.GetSolidEdgeMiddle(direction) * HexMetrics.InnerToOuter;
                roadCenter += offset * 0.7f;
                center += offset * 0.5f;
            } else {
                HexDirection middle;
                if (previousHasRiver) {
                    middle = direction.Next();
                } else if (nextHasRiver) {
                    middle = direction.Previous();
                } else {
                    middle = direction;
                }
                if (!cell.HasRoadThroughEdge(middle) && !cell.HasRoadThroughEdge(middle.Previous()) && !cell.HasRoadThroughEdge(middle.Next())) return;
                var offset = HexMetrics.GetSolidEdgeMiddle(middle);
                roadCenter += offset * 0.25f;
                if (direction == middle && cell.HasRoadThroughEdge(direction.Opposite()))
                    features.AddBridge(roadCenter, center - offset * (HexMetrics.InnerToOuter * 0.7f));
            }
            
            var mL = Vector3.Lerp(roadCenter, e.v1, interpolators.x);
            var mR = Vector3.Lerp(roadCenter, e.v5, interpolators.y);
            if (previousHasRiver) TriangulateRoadEdge(roadCenter, center, mL);
            if (nextHasRiver) TriangulateRoadEdge(roadCenter, mR, center);
            TriangulateRoad(roadCenter, mL, mR, e, hasRoadThroughEdge);
        } // TriangulateRoadAdjacentToRiver ()
        
        private void TriangulateRoadEdge (Vector3 center, Vector3 mL, Vector3 mR) {
            roads.AddTriangle(center, mL, mR);
            roads.AddTriangleUV(new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f));
        } // TriangulateRoadEdge ()
        
        private void TriangulateConnection(HexDirection direction, HexCell cell, EdgeVertices e1) {
            var neighbor = cell.GetNeighbor(direction);
            if (neighbor == null) return;

            var bridge = HexMetrics.GetBridge(direction);
            bridge.y = neighbor.Position.y - cell.Position.y;
            var e2 = new EdgeVertices(e1.v1 + bridge, e1.v5 + bridge);

            var hasRiver = cell.HasRiverThroughEdge(direction);
            var hasRoad = cell.HasRoadThroughEdge(direction);
            
            if (hasRiver) {
                e2.v3.y = neighbor.StreamBedY;

                if (!cell.IsUnderwater) {
                    if (!neighbor.IsUnderwater) {
                        TriangulateRiverQuad(e1.v2, e1.v4, e2.v2, e2.v4, cell.RiverSurfaceY, neighbor.RiverSurfaceY, 0.8f, cell.HasIncomingRiver && cell.IncomingRiver == direction);
                    } else if (cell.Elevation > neighbor.WaterLevel) {
                        TriangulateWaterfallInWater(e1.v2, e1.v4, e2.v2, e2.v4, cell.RiverSurfaceY, neighbor.RiverSurfaceY, neighbor.WaterSurfaceY);
                    }
                } else if (!neighbor.IsUnderwater && neighbor.Elevation > cell.WaterLevel) {
                    TriangulateWaterfallInWater(e2.v4, e2.v2, e1.v4, e1.v2, neighbor.RiverSurfaceY, cell.RiverSurfaceY, cell.WaterSurfaceY);
                }
            }

            if (cell.GetEdgeType(direction) == HexEdgeType.Slope) {
                TriangulateEdgeTerraces(e1, cell, e2, neighbor, hasRoad);
            } else {
                TriangulateEdgeStrip(e1, m_color1, cell.TerrainTypeIndex, e2, m_color2, neighbor.TerrainTypeIndex, hasRoad);
            }
            
            features.AddWall(e1, cell, e2, neighbor, hasRiver, hasRoad);

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
            var leftEdgeType = bottomCell.GetEdgeType(leftCell);
            var rightEdgeType = bottomCell.GetEdgeType(rightCell);

            if (leftEdgeType == HexEdgeType.Slope) {
                if (rightEdgeType == HexEdgeType.Slope) {
                    TriangulateCornerTerraces(bottom, bottomCell, left, leftCell, right, rightCell);
                } else if (rightEdgeType == HexEdgeType.Flat) {
                    TriangulateCornerTerraces(left, leftCell, right, rightCell, bottom, bottomCell);
                } else {
                    TriangulateCornerTerracesCliff(bottom, bottomCell, left, leftCell, right, rightCell);
                }
            } else if (rightEdgeType == HexEdgeType.Slope) {
                if (leftEdgeType == HexEdgeType.Flat) {
                    TriangulateCornerTerraces(right, rightCell, bottom, bottomCell, left, leftCell);
                } else {
                    TriangulateCornerCliffTerraces(bottom, bottomCell, left, leftCell, right, rightCell);
                }
            } else if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope) {
                if (leftCell.Elevation < rightCell.Elevation) {
                    TriangulateCornerCliffTerraces(right, rightCell, bottom, bottomCell, left, leftCell);
                } else {
                    TriangulateCornerTerracesCliff(left, leftCell, right, rightCell, bottom, bottomCell);
                }
            } else {
                terrain.AddTriangle(bottom, left, right);
                terrain.AddTriangleColor(m_color1, m_color2, m_color3);
                Vector3 types;
                types.x = bottomCell.TerrainTypeIndex;
                types.y = leftCell.TerrainTypeIndex;
                types.z = rightCell.TerrainTypeIndex;
                terrain.AddTriangleTerrainTypes(types);
            }
            
            features.AddWall(bottom, bottomCell, left, leftCell, right, rightCell);
        } // TriangulateCorner ()

        private void TriangulateEdgeTerraces(EdgeVertices begin, HexCell beginCell, EdgeVertices end, HexCell endCell, bool hasRoad) {
            var e2 = EdgeVertices.TerraceLerp(begin, end, 1);
            var c2 = HexMetrics.TerraceLerp(m_color1, m_color2, 1);
            float t1 = beginCell.TerrainTypeIndex;
            float t2 = endCell.TerrainTypeIndex;

            TriangulateEdgeStrip(begin, m_color1, t1, e2, c2, t2, hasRoad);

            for (var i = 2; i < HexMetrics.TerraceSteps; i++) {
                var e1 = e2;
                var c1 = c2;
                e2 = EdgeVertices.TerraceLerp(begin, end, i);
                c2 = HexMetrics.TerraceLerp(m_color1, m_color2, i);
                TriangulateEdgeStrip(e1, c1, t1, e2, c2, t2, hasRoad);
            }

            TriangulateEdgeStrip(e2, c2, t1, end, m_color2, t2, hasRoad);
        } // TriangulateEdgeTerraces ()

        private void TriangulateCornerTerraces(Vector3 begin, HexCell beginCell, Vector3 left, HexCell leftCell, Vector3 right, HexCell rightCell) {
            var v3 = HexMetrics.TerraceLerp(begin, left, 1);
            var v4 = HexMetrics.TerraceLerp(begin, right, 1);
            var c3 = HexMetrics.TerraceLerp(m_color1, m_color2, 1);
            var c4 = HexMetrics.TerraceLerp(m_color1, m_color3, 1);
            Vector3 types;
            types.x = beginCell.TerrainTypeIndex;
            types.y = leftCell.TerrainTypeIndex;
            types.z = rightCell.TerrainTypeIndex;
            
            terrain.AddTriangle(begin, v3, v4);
            terrain.AddTriangleColor(m_color1, c3, c4);
            terrain.AddTriangleTerrainTypes(types);

            for (var i = 2; i < HexMetrics.TerraceSteps; i++) {
                var v1 = v3;
                var v2 = v4;
                var c1 = c3;
                var c2 = c4;
                v3 = HexMetrics.TerraceLerp(begin, left, i);
                v4 = HexMetrics.TerraceLerp(begin, right, i);
                c3 = HexMetrics.TerraceLerp(m_color1, m_color2, i);
                c4 = HexMetrics.TerraceLerp(m_color1, m_color3, i);
                terrain.AddQuad(v1, v2, v3, v4);
                terrain.AddQuadColor(c1, c2, c3, c4);
                terrain.AddQuadTerrainTypes(types);
            }

            terrain.AddQuad(v3, v4, left, right);
            terrain.AddQuadColor(c3, c4, m_color2, m_color3);
            terrain.AddQuadTerrainTypes(types);
        } // TriangulateCornerTerraces ()

        private void TriangulateCornerTerracesCliff(Vector3 begin, HexCell beginCell, Vector3 left, HexCell leftCell, Vector3 right, HexCell rightCell) {
            var b = 1f / (rightCell.Elevation - beginCell.Elevation);
            if (b < 0) b = -b;

            var boundary = Vector3.Lerp(HexMetrics.Perturb(begin), HexMetrics.Perturb(right), b);
            var boundaryColor = Color.Lerp(m_color1, m_color3, b);
            Vector3 types;
            types.x = beginCell.TerrainTypeIndex;
            types.y = leftCell.TerrainTypeIndex;
            types.z = rightCell.TerrainTypeIndex;

            TriangulateBoundaryTriangle(begin, m_color1, left, m_color2, boundary, boundaryColor, types);

            if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope) { 
                TriangulateBoundaryTriangle(left, m_color2, right, m_color3, boundary, boundaryColor, types);
            } else {
                terrain.AddTriangleUnperturbed(HexMetrics.Perturb(left), HexMetrics.Perturb(right), boundary);
                terrain.AddTriangleColor(m_color2, m_color3, boundaryColor);
                terrain.AddTriangleTerrainTypes(types);
            }
        } // TriangulateCornerTerracesCliff ()

        private void TriangulateCornerCliffTerraces(Vector3 begin, HexCell beginCell, Vector3 left, HexCell leftCell, Vector3 right, HexCell rightCell) {
            var b = 1f / (leftCell.Elevation - beginCell.Elevation);
            if (b < 0) b = -b;

            var boundary = Vector3.Lerp(HexMetrics.Perturb(begin), HexMetrics.Perturb(left), b);
            var boundaryColor = Color.Lerp(m_color1, m_color2, b);
            Vector3 types;
            types.x = beginCell.TerrainTypeIndex;
            types.y = leftCell.TerrainTypeIndex;
            types.z = rightCell.TerrainTypeIndex;

            TriangulateBoundaryTriangle(right, m_color3, begin, m_color1, boundary, boundaryColor, types);

            if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope) {
                TriangulateBoundaryTriangle(left, m_color2, right, m_color3, boundary, boundaryColor, types);
            } else {
                terrain.AddTriangleUnperturbed(HexMetrics.Perturb(left), HexMetrics.Perturb(right), boundary);
                terrain.AddTriangleColor(m_color2, m_color3, boundaryColor);
                terrain.AddTriangleTerrainTypes(types);
            }
        } // TriangulateCornerCliffTerraces ()

        private void TriangulateBoundaryTriangle(Vector3 begin, Color beginColor, Vector3 left, Color leftColor, Vector3 boundary, Color boundaryColor, Vector3 types) {
            var v2 = HexMetrics.Perturb(HexMetrics.TerraceLerp(begin, left, 1));
            var c2 = HexMetrics.TerraceLerp(beginColor, leftColor, 1);

            terrain.AddTriangleUnperturbed(HexMetrics.Perturb(begin), v2, boundary);
            terrain.AddTriangleColor(beginColor, c2, boundaryColor);
            terrain.AddTriangleTerrainTypes(types);

            for (var i = 2; i < HexMetrics.TerraceSteps; i++) {
                var v1 = v2;
                var c1 = c2;
                v2 = HexMetrics.Perturb(HexMetrics.TerraceLerp(begin, left, i));
                c2 = HexMetrics.TerraceLerp(beginColor, leftColor, i);
                terrain.AddTriangleUnperturbed(v1, v2, boundary);
                terrain.AddTriangleColor(c1, c2, boundaryColor);
                terrain.AddTriangleTerrainTypes(types);
            }

            terrain.AddTriangleUnperturbed(v2, HexMetrics.Perturb(left), boundary);
            terrain.AddTriangleColor(c2, leftColor, boundaryColor);
            terrain.AddTriangleTerrainTypes(types);
        } // TriangulateBoundaryTriangle ()

        private void TriangulateEdgeFan(Vector3 center, EdgeVertices edge, float type) {
            terrain.AddTriangle(center, edge.v1, edge.v2);
            terrain.AddTriangle(center, edge.v2, edge.v3);
            terrain.AddTriangle(center, edge.v3, edge.v4);
            terrain.AddTriangle(center, edge.v4, edge.v5);
            
            terrain.AddTriangleColor(m_color1);
            terrain.AddTriangleColor(m_color1);
            terrain.AddTriangleColor(m_color1);
            terrain.AddTriangleColor(m_color1);
            
            Vector3 types;
            types.x = types.y = types.z = type;
            terrain.AddTriangleTerrainTypes(types);
            terrain.AddTriangleTerrainTypes(types);
            terrain.AddTriangleTerrainTypes(types);
            terrain.AddTriangleTerrainTypes(types);
        } // TriangulateEdgeFan ()

        private void TriangulateEdgeStrip(EdgeVertices e1, Color c1, float type1, EdgeVertices e2, Color c2, float type2, bool hasRoad = false) {
            terrain.AddQuad(e1.v1, e1.v2, e2.v1, e2.v2);
            terrain.AddQuad(e1.v2, e1.v3, e2.v2, e2.v3);
            terrain.AddQuad(e1.v3, e1.v4, e2.v3, e2.v4);
            terrain.AddQuad(e1.v4, e1.v5, e2.v4, e2.v5);

            terrain.AddQuadColor(c1, c2);
            terrain.AddQuadColor(c1, c2);
            terrain.AddQuadColor(c1, c2);
            terrain.AddQuadColor(c1, c2);

            Vector3 types;
            types.x = types.z = type1;
            types.y = type2;
            terrain.AddQuadTerrainTypes(types);
            terrain.AddQuadTerrainTypes(types);
            terrain.AddQuadTerrainTypes(types);
            terrain.AddQuadTerrainTypes(types);
            
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