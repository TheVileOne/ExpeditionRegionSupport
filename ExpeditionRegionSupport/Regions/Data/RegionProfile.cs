using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ExpeditionRegionSupport.Regions.Data
{
    public readonly struct RegionProfile : ITreeNode
    {
        /// <summary>
        /// This value represents the cache identifier that this profile belongs to
        /// </summary>
        public readonly int RegisterID = -1;

        public readonly string RegionCode;

        /// <summary>
        /// Contains regions considered equivalent, but are replaced by this region under one of more conditions
        /// </summary>
        public readonly List<RegionProfile> EquivalentBaseRegions = new List<RegionProfile>();

        /// <summary>
        /// Contains regions considered equivalent replacements for this region under one or more conditions.
        /// Conditions are organized by the slugcat name
        /// </summary>
        public readonly Dictionary<SlugcatStats.Name, RegionProfile> EquivalentRegions = new Dictionary<SlugcatStats.Name, RegionProfile>();

        /// <summary>
        /// This region does not replace any other equivalent regions
        /// </summary>
        public bool IsBaseRegion => EquivalentBaseRegions.Count == 0;

        /// <summary>
        /// This region should not be assigned any base equivalent regions even if it serves as a substitute for another region  
        /// </summary>
        public readonly bool IsPermanentBaseRegion;

        public bool HasEquivalentRegions => !IsBaseRegion || EquivalentRegions.Any();

        public bool IsDefault => Equals(default(RegionProfile));

        /// <summary>
        /// Stores the RegionProfile requesting to be made an equivalency to another RegionProfile
        /// </summary>
        public readonly StrongBox<RegionProfile> PendingEquivalency;

        /// <summary>
        /// This RegionProfile is part of a recursive validation check
        /// </summary>
        public readonly StrongBox<bool> ValidationInProgress;

        /// <summary>
        /// Determines if RegionProfile node children refer to the base equivalences, or the equivalences stored in EquivalentRegions 
        /// </summary>
        public static bool NodeTreeForward = false;

        public RegionProfile(int registerID = -1)
        {
            RegisterID = registerID;
            PendingEquivalency = new StrongBox<RegionProfile>();
            ValidationInProgress = new StrongBox<bool>();
        }

        public RegionProfile(string regionCode, int registerID = -1) : this(registerID)
        {
            RegionCode = regionCode;
            IsPermanentBaseRegion = RegionUtils.IsVanillaRegion(RegionCode)
                                 || RegionCode == "OE" || RegionCode == "VS" || RegionCode == "HR" || RegionCode == "MS" || RegionCode == "LC";
        }

        public bool HasEquivalencyEntry(SlugcatStats.Name slugcat, RegionProfile regionProfile)
        {
            bool entryFound = false;
            if (EquivalentRegions.TryGetValue(slugcat, out RegionProfile foundProfile))
                entryFound = foundProfile.RegionCode == regionProfile.RegionCode;

            return entryFound;
        }

        public bool IsSlugcatAllowedHere(SlugcatStats.Name slugcat)
        {
            if (IsDefault) return false;
            if (IsBaseRegion) return true; //Base regions cannot have slugcat restrictions

            ///Checks whether the base equivalent relationships with this profile would allow this slugcat to enter this region from a gate
            RegionProfile thisProfile = this;
            return EquivalentBaseRegions.Any(p => p.HasEquivalencyEntry(slugcat, thisProfile) || p.HasEquivalencyEntry(SlugcatUtils.AnySlugcat, thisProfile));
        }

        /// <summary>
        /// Establish an equivalent relationship with another RegionProfile
        /// </summary>
        /// <param name="slugcat">The slugcat conditions for loading a specific equivalent region</param>
        /// <param name="region">The equivalent region that will be loaded based on a specific slugcat</param>
        public void RegisterEquivalency(SlugcatStats.Name slugcat, RegionProfile region)
        {
            if (IsDefault)
            {
                string errorMessage = "Default RegionProfile does not accept registrations";
                if (Plugin.DebugMode)
                    throw new InvalidOperationException(errorMessage);
                Plugin.Logger.LogError(errorMessage);
                return;
            }

            if (slugcat == null)
                throw new ArgumentNullException(nameof(slugcat), nameof(RegisterEquivalency) + " encountered an exception: Slugcat cannot be null");

            if (region.Equals(this) || region.IsDefault || EquivalentBaseRegions.Contains(region)) return;

            //Check that this region already has an equivalent region assigned to this slugcat
            if (EquivalentRegions.TryGetValue(slugcat, out RegionProfile existingProfile))
            {
                //Don't process if region has already been assigned for this slugcat
                if (!existingProfile.IsDefault)
                {
                    //Current code does not support overwriting equivalences
                    string reportMessage = existingProfile.Equals(region) ? "Skipping duplicate equivalency" : "Applying equivalency would overwrite existing entry";
                    Plugin.Logger.LogInfo(reportMessage);
                    return;
                }

                Plugin.Logger.LogWarning("EquivalentRegions should not have empty values");
                EquivalentRegions.Remove(slugcat);
            }

            if (Plugin.DebugMode)
            {
                Plugin.Logger.LogInfo("Registration");
                Plugin.Logger.LogInfo("Slugcat: " + slugcat);
                Plugin.Logger.LogInfo("Region targeted: " + region.RegionCode);
            }

            PendingEquivalency.Value = region;

            if (!HasIllegalRelationships(region, slugcat))
            {
                Plugin.Logger.LogInfo("Applying equivalency relationship targeting " + RegionCode);
                EquivalentRegions[slugcat] = region;

                if (!region.IsPermanentBaseRegion && !region.EquivalentBaseRegions.Contains(this)) //Prevent certain regions from having base equivalencies
                    region.EquivalentBaseRegions.Add(this);
            }

            PendingEquivalency.Value = default;
        }

        /// <summary>
        /// Check for situations that would make an equivalency relationship with this region redundant, or incompatible 
        /// </summary>
        public bool HasIllegalRelationships(RegionProfile region, SlugcatStats.Name slugcat)
        {
            if (IsDefault) return true; //Default profile does not have established relationships

            //This is stored in the registering RegionProfile (the callback region), and is required to ensure complicated, but possible loop relationships are detected
            if (!region.PendingEquivalency.Value.IsDefault && (slugcat == SlugcatUtils.AnySlugcat || !EquivalentRegions.ContainsKey(slugcat)))
                region = region.PendingEquivalency.Value;

            //The AnySlugcat specification requires all slugcat relationships to be checked for illegal relationships
            if (slugcat == SlugcatUtils.AnySlugcat)
            {
                try
                {
                    /* 
                     * Encountering this flag already set to true indicates that this RegionProfile has already had its AnySlugcat relationship check earlier in the recursive process
                     * This situation is always invalid as this indicates there is an illegal loop formed by AnySlugcat equivalency relationships. It will also cause this algorithm to
                     * endlessly check the same regions if we don't return here.
                     */
                    if (ValidationInProgress.Value)
                        return true;

                    ValidationInProgress.Value = true;

                    //Checks the equivalent region data of the region we want to process to see if it can reach the callback region, which would form an illegal loop 
                    RegionProfile callbackRegion = this;
                    return region.EquivalentRegions.Exists(equivalencyEntry => callbackRegion.HasIllegalRelationships(equivalencyEntry.Value, equivalencyEntry.Key));
                }
                finally
                {
                    ValidationInProgress.Value = false;
                }
            }

            RegionProfile compareRegion = region;

            bool continueLoop = true;
            bool hasIllegalRelationships = false;
            do
            {
                RegionProfile regionCheck = region.GetRegionCandidate(slugcat);

                if (regionCheck.IsDefault) //The slugcat is not associated with any additional equivalent regions
                {
                    continueLoop = false;
                }
                else if (regionCheck.Equals(this) || regionCheck.Equals(compareRegion)) //Compare each subsequent region to either the callback region, or its pending equivalency
                {
                    hasIllegalRelationships = true; //Illegal loop detected - Actually there is a form of loop that wouldn't be illegal, but could lead to a different region being
                    continueLoop = false;           //loaded when the region code gets changes to its base equivalent region profile.
                }

                region = regionCheck; //Set the next profile to evaluate
            }
            while (continueLoop);

            return hasIllegalRelationships;
        }

        /// <summary>
        /// Gets the region profile of the region considered equivalent to this profile for a specific slugcat
        /// </summary>
        public RegionProfile GetSlugcatEquivalentRegion(SlugcatStats.Name slugcat)
        {
            if (IsDefault) return this;

            slugcat = NormalizeSlugcatInput(slugcat);

            if (slugcat == SlugcatUtils.AnySlugcat) //There is no practical way of determining which region should be returned in this circumstance
                return this;

            return InternalGetSlugcatEquivalentRegion(slugcat, out _);
        }

        /// <summary>
        /// Gets the region profile of the region considered equivalent to this profile for a specific slugcat
        /// </summary>
        public RegionProfile GetSlugcatEquivalentRegion(SlugcatStats.Name slugcat, out RegionProfile regionBaseEquivalent)
        {
            if (IsDefault)
            {
                regionBaseEquivalent = default;
                return this;
            }

            slugcat = NormalizeSlugcatInput(slugcat);

            if (slugcat == SlugcatUtils.AnySlugcat) //There is no practical way of determining which region should be returned in this circumstance
            {
                regionBaseEquivalent = GetEquivalentBaseRegion();
                return this;
            }

            return InternalGetSlugcatEquivalentRegion(slugcat, out regionBaseEquivalent);
        }

        /// <summary>
        /// Gets the base equivalent region that most closely associates with a vanilla/downpour region, and otherwise has no other equivalent regions
        /// </summary>
        public RegionProfile GetEquivalentBaseRegion()
        {
            if (IsBaseRegion || IsDefault)
                return this;

            if (EquivalentBaseRegions.Count > 1)
            {
                Plugin.Logger.LogInfo($"Multiple base regions for {RegionCode} detected. Choosing one");

                //Regions with more lenient restrictions should be prioritized over regions with only slugcat-specific restrictions
                RegionProfile thisProfile = this;
                RegionProfile baseProfile = EquivalentBaseRegions.Find(r => r.EquivalentRegions.Exists(checkEntry));

                if (!baseProfile.IsDefault)
                    return baseProfile;

                bool checkEntry(KeyValuePair<SlugcatStats.Name, RegionProfile> equivalencyEntry)
                {
                    return equivalencyEntry.Key == SlugcatUtils.AnySlugcat && equivalencyEntry.Value.Equals(thisProfile);
                }
            }

            return EquivalentBaseRegions[0].GetEquivalentBaseRegion(); //Default to the first registered base
        }

        public void GetAllEquivalentBaseRegions(int recursiveSteps, out RegionProfile[][] baseRegionMap)
        {
            recursiveSteps++;
            if (IsBaseRegion || IsDefault)
            {
                baseRegionMap = new RegionProfile[recursiveSteps][];
                return;
            }

            EquivalentBaseRegions[0].GetAllEquivalentBaseRegions(recursiveSteps, out baseRegionMap);

            int currentBaseTier = baseRegionMap[0].Length - recursiveSteps; //Equivalency tiers start at the lowest base, and increase by 1 with each additional step 
            baseRegionMap[currentBaseTier] = EquivalentBaseRegions.ToArray();
        }

        /// <summary>
        /// Gets the base equivalent region that most closely associated with a vanilla/downpour region for a specified slugcat.
        /// This will probably return the same result in most cases. The main difference is that this method prioritizes base regions
        /// that target a specific slugcat.
        /// </summary>
        public RegionProfile GetEquivalentBaseRegion(SlugcatStats.Name slugcat)
        {
            slugcat = NormalizeSlugcatInput(slugcat);

            if (slugcat == SlugcatUtils.AnySlugcat)
                return GetEquivalentBaseRegion();

            return InternalGetEquivalentBaseRegion(slugcat);
        }

        #region Internal Methods

        internal RegionProfile InternalGetSlugcatEquivalentRegion(SlugcatStats.Name slugcat, out RegionProfile baseEquivalentRegion)
        {
            baseEquivalentRegion = InternalGetEquivalentBaseRegion(slugcat); //Region candidacy checking should start at a base equivalent region
            return baseEquivalentRegion.GetRegionCandidateRecursive(slugcat);
        }

        internal RegionProfile InternalGetEquivalentBaseRegion(SlugcatStats.Name slugcat)
        {
            if (IsBaseRegion || IsDefault)
                return this;

            if (EquivalentBaseRegions.Count > 1)
            {
                Plugin.Logger.LogInfo($"Multiple base regions for {RegionCode} detected. Choosing one");

                RegionProfile mostRelevantBaseRegion = EquivalentBaseRegions.Find(r => r.EquivalentRegions.ContainsKey(slugcat));

                if (!mostRelevantBaseRegion.IsDefault)
                    return mostRelevantBaseRegion;
            }

            return EquivalentBaseRegions[0].InternalGetEquivalentBaseRegion(slugcat); //Default to the first registered base
        }

        /// <summary>
        /// Gets the most relevant equivalent region that is compatible with a specified slugcat
        /// </summary>
        internal RegionProfile GetRegionCandidate(SlugcatStats.Name slugcat)
        {
            if (slugcat == SlugcatUtils.AnySlugcat)
            {
                if (Plugin.DebugMode)
                    throw new NotSupportedException("A slugcat must be specified for this operation");

                Plugin.Logger.LogWarning("Returning a region candidate for any slugcat. Is this intentional?");
                return EquivalentRegions.FirstOrDefault().Value;
            }

            //Check that there is an equivalent region specific to this slugcat
            EquivalentRegions.TryGetValue(slugcat, out RegionProfile equivalentRegion);

            //If there are no slugcat specific equivalent regions, check for any unspecific equivalent regions
            if (equivalentRegion.IsDefault)
                EquivalentRegions.TryGetValue(SlugcatUtils.AnySlugcat, out equivalentRegion);

            return equivalentRegion;
        }

        internal RegionProfile GetRegionCandidateRecursive(SlugcatStats.Name slugcat)
        {
            RegionProfile equivalentRegion = GetRegionCandidate(slugcat);

            if (!equivalentRegion.IsDefault)
            {
                //We found a valid equivalent region, but we're not finished. Check if that equivalent region has equivalent regions
                return equivalentRegion.GetRegionCandidateRecursive(slugcat);
            }
            return this; //Return this when this is the most valid equivalent region
        }

        /// <summary>
        /// Changes slugcat data into an expected format
        /// </summary>
        internal static SlugcatStats.Name NormalizeSlugcatInput(SlugcatStats.Name slugcat)
        {
            return slugcat ?? SlugcatUtils.AnySlugcat; //Convert null value into another form
        }

        #endregion

        /// <summary>
        /// Provide a list of all current equivalencies for this region
        /// </summary>
        public List<RegionProfile> ListEquivalences(bool includeSelf)
        {
            Tree searchTree = new Tree(this);

            //Fetch equivalences that this region replaces
            IEnumerable<RegionProfile> searchResult = searchTree.GetAllNodes<RegionProfile>();

            List<RegionProfile> listResult = new List<RegionProfile>();
            listResult.AddRange(searchResult);

            //Fetch equivalences that replace this region
            NodeTreeForward = true;
            searchResult = searchTree.GetAllNodes<RegionProfile>();
            listResult.AddRange(searchResult.Except(listResult));

            NodeTreeForward = false;

            if (!includeSelf)
                listResult.Remove(this);
            return listResult;
        }

        public void LogEquivalences(bool logOnlyIfDataExists)
        {
            if (IsDefault) return;

            if (IsBaseRegion)
            {
                bool hasData = EquivalentRegions.Count > 0;
                if (hasData || !logOnlyIfDataExists)
                {
                    Plugin.Logger.LogInfo("Equivalency info for " + RegionCode);
                    Plugin.Logger.LogInfo(hasData ? "Has slugcat conditions" : "NONE");
                }
                return;
            }

            Plugin.Logger.LogInfo("Equivalency info for " + RegionCode);
            Plugin.Logger.LogInfo("Base equivalences");
            Tree regionTree = new Tree(this);
            regionTree.OnDataLogged += onDataLogged; //There is specific information that we want to know that doesn't get logged through Tree

            regionTree.LogData();
        }

        public override string ToString()
        {
            return RegionCode;
        }

        public IEnumerable<ITreeNode> GetChildNodes()
        {
            IEnumerable<RegionProfile> nextNodeTier;

            if (NodeTreeForward)
                nextNodeTier = EquivalentRegions.Values; //Ignore slugcat restrictions here
            else
                nextNodeTier = EquivalentBaseRegions;
            return nextNodeTier.Select(node => node as ITreeNode);
        }

        private void onDataLogged(IEnumerable<ITreeNode> nodesAtThisDepth, int nodeDepth)
        {
            if (nodeDepth == 0 || !nodesAtThisDepth.Any()) return; //At depth 0, EquivalentRegions will always be empty

            int nodeCount = nodesAtThisDepth.Count();
            foreach (RegionProfile node in nodesAtThisDepth)
            {
                string conditionsHeader = nodeCount == 1 ? "Slugcat Conditions" : $"Slugcat Conditions ({node.RegionCode})";

                Plugin.Logger.LogInfo(conditionsHeader);
                Plugin.Logger.LogInfo(node.EquivalentRegions.Select(r => r.Key).FormatToString(','));
            }
        }
    }
}
