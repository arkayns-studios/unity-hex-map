using UnityEngine;

namespace Arkayns.Reckon.HM {

    public class HexFeatureManager : MonoBehaviour {

        // -- Variables --
        public HexFeatureCollection[] urbanCollections, farmCollections, plantCollections;
        public HexMesh walls;
        public Transform wallTower;
        private Transform m_container;
        
        // -- Methods --
        public void Clear() {
            if (m_container) {
                Destroy(m_container.gameObject);
            }
            m_container = new GameObject("Features Container").transform;
            m_container.SetParent(transform, false);
            
            walls.Clear();
        } // Clear ()

        public void Apply() {
            walls.Apply();
        } // Apply ()

        private Transform PickPrefab (HexFeatureCollection[] collection, int level, float hash, float choice) {
            if (level <= 0) return null;
            var thresholds = HexMetrics.GetFeatureThresholds(level - 1);
            for (var i = 0; i < thresholds.Length; i++) {
                if (hash < thresholds[i]) return collection[i].Pick(choice);
            }
            return null;
        } // PickPrefab ()
        
        public void AddFeature(HexCell cell, Vector3 position) {
            var hash = HexMetrics.SampleHashGrid(position);
            var prefab = PickPrefab(urbanCollections, cell.UrbanLevel, hash.a, hash.d);
            var otherPrefab = PickPrefab(farmCollections, cell.FarmLevel, hash.b, hash.d);
            
            var usedHash = hash.a;
            if (prefab) {
                if (otherPrefab && hash.b < hash.a) {
                    prefab = otherPrefab;
                    usedHash = hash.b;
                }
            } else if (otherPrefab) {
                prefab = otherPrefab;
                usedHash = hash.b;
            }
            
            otherPrefab = PickPrefab(plantCollections, cell.PlantLevel, hash.c, hash.d);
            if (prefab) {
                if (otherPrefab && hash.c < usedHash) {
                    prefab = otherPrefab;
                }
            } else if (otherPrefab) {
                prefab = otherPrefab;
            } else {
                return;
            }
            
            var instance = Instantiate(prefab, m_container, false);
            position.y += instance.localScale.y * 0.5f;
            instance.localPosition = HexMetrics.Perturb(position);
            instance.localRotation = Quaternion.Euler(0f, 360f * hash.e, 0f);
        } // AddFeature ()

        public void AddWall (EdgeVertices near, HexCell nearCell, EdgeVertices far, HexCell farCell, bool hasRiver, bool hasRoad) {
            if (nearCell.Walled != farCell.Walled && !nearCell.IsUnderwater && !farCell.IsUnderwater && nearCell.GetEdgeType(farCell) != HexEdgeType.Cliff) {
                AddWallSegment(near.v1, far.v1, near.v2, far.v2);
                if (hasRiver || hasRoad) {
                    AddWallCap(near.v2, far.v2);
                    AddWallCap(far.v4, near.v4);
                } else {
                    AddWallSegment(near.v2, far.v2, near.v3, far.v3);
                    AddWallSegment(near.v3, far.v3, near.v4, far.v4);
                }
                AddWallSegment(near.v4, far.v4, near.v5, far.v5);
            }
        } // AddWall ()
        
        public void AddWall (Vector3 c1, HexCell cell1, Vector3 c2, HexCell cell2, Vector3 c3, HexCell cell3) {
            if (cell1.Walled) {
                if (cell2.Walled) {
                    if (!cell3.Walled) {
                        AddWallSegment(c3, cell3, c1, cell1, c2, cell2);
                    }
                } else if (cell3.Walled) {
                    AddWallSegment(c2, cell2, c3, cell3, c1, cell1);
                } else {
                    AddWallSegment(c1, cell1, c2, cell2, c3, cell3);
                }
            } else if (cell2.Walled) {
                if (cell3.Walled) {
                    AddWallSegment(c1, cell1, c2, cell2, c3, cell3);
                } else {
                    AddWallSegment(c2, cell2, c3, cell3, c1, cell1);
                }
            } else if (cell3.Walled) {
                AddWallSegment(c3, cell3, c1, cell1, c2, cell2);
            }
        } // AddWall ()
        
        private void AddWallSegment (Vector3 pivot, HexCell pivotCell, Vector3 left, HexCell leftCell, Vector3 right, HexCell rightCell) {
            if (pivotCell.IsUnderwater) return;
            
            var hasLeftWall = !leftCell.IsUnderwater && pivotCell.GetEdgeType(leftCell) != HexEdgeType.Cliff;
            var hasRighWall = !rightCell.IsUnderwater && pivotCell.GetEdgeType(rightCell) != HexEdgeType.Cliff;
            
            if (hasLeftWall) {
                if(hasRighWall) {
                    var hasTower = false;
                    if (leftCell.Elevation == rightCell.Elevation) {
                        var hash = HexMetrics.SampleHashGrid((pivot + left + right) * (1f / 3f));
                        hasTower = hash.e < HexMetrics.WallTowerThreshold;
                    }
                    AddWallSegment(pivot, left, pivot, right, hasTower);
                } else if (leftCell.Elevation < rightCell.Elevation) {
                    AddWallWedge(pivot, left, right);
                } else AddWallCap(pivot, left);
            } else if (hasRighWall) {
                if (rightCell.Elevation < leftCell.Elevation) {
                    AddWallWedge(right, pivot, left);
                } else {
                    AddWallCap(right, pivot);
                }
            }
        } // AddWallSegment ()
        
        private void AddWallSegment (Vector3 nearLeft, Vector3 farLeft, Vector3 nearRight, Vector3 farRight, bool addTower = false) {
            nearLeft = HexMetrics.Perturb(nearLeft);
            farLeft = HexMetrics.Perturb(farLeft);
            nearRight = HexMetrics.Perturb(nearRight);
            farRight = HexMetrics.Perturb(farRight);
            
            var left = HexMetrics.WallLerp(nearLeft, farLeft);
            var right = HexMetrics.WallLerp(nearRight, farRight);
            
            var leftThicknessOffset = HexMetrics.WallThicknessOffset(nearLeft, farLeft);
            var rightThicknessOffset = HexMetrics.WallThicknessOffset(nearRight, farRight);
            
            var leftTop = left.y + HexMetrics.WallHeight;
            var rightTop = right.y + HexMetrics.WallHeight;

            Vector3 v1, v2, v3, v4;
            v1 = v3 = left - leftThicknessOffset;
            v2 = v4 = right - rightThicknessOffset;
            v3.y = leftTop;
            v4.y = rightTop;
            walls.AddQuadUnperturbed(v1, v2, v3, v4);

            Vector3 t1 = v3, t2 = v4;
            v1 = v3 = left + leftThicknessOffset;
            v2 = v4 = right + rightThicknessOffset;
            v3.y = leftTop;
            v4.y = rightTop;
            walls.AddQuadUnperturbed(v2, v1, v4, v3);
            
            walls.AddQuadUnperturbed(t1, t2, v3, v4);

            if (addTower) {
                var towerInstance = Instantiate(wallTower);
                towerInstance.transform.localPosition = (left + right) * 0.5f;
                var rightDirection = right - left;
                rightDirection.y = 0f;
                towerInstance.transform.right = rightDirection;
                towerInstance.SetParent(m_container, false);
            }
        } // AddWallSegment ()

        private void AddWallCap (Vector3 near, Vector3 far) {
            near = HexMetrics.Perturb(near);
            far = HexMetrics.Perturb(far);

            var center = HexMetrics.WallLerp(near, far);
            var thickness = HexMetrics.WallThicknessOffset(near, far);

            Vector3 v1, v2, v3, v4;
            v1 = v3 = center - thickness;
            v2 = v4 = center + thickness;
            v3.y = v4.y = center.y + HexMetrics.WallHeight;
            walls.AddQuadUnperturbed(v1, v2, v3, v4);
        } // AddWallCap ()
        
        private void AddWallWedge (Vector3 near, Vector3 far, Vector3 point) {
            near = HexMetrics.Perturb(near);
            far = HexMetrics.Perturb(far);
            point = HexMetrics.Perturb(point);

            var center = HexMetrics.WallLerp(near, far);
            var thickness = HexMetrics.WallThicknessOffset(near, far);

            Vector3 v1, v2, v3, v4;
            var pointTop = point;
            point.y = center.y;

            v1 = v3 = center - thickness;
            v2 = v4 = center + thickness;
            v3.y = v4.y = pointTop.y = center.y + HexMetrics.WallHeight;

            walls.AddQuadUnperturbed(v1, point, v3, pointTop);
            walls.AddQuadUnperturbed(point, v2, pointTop, v4);
            walls.AddTriangleUnperturbed(pointTop, v3, v4);
        } // AddWallWedge ()
        
    } // Class HexFeatureManager

} // Namespace Arkayns Reckon HM