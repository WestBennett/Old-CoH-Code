using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DeGeotizer
{
    public class Vec2
    {
        float x, y;
        
        Vec2(float X, float Y)
        {
            x = X;
            y = Y;
        }
    }

    public class Vec3
    {
        float x, y, z;

        Vec3(float X, float Y, float Z)
        {
            x = X;
            y = Y;
            z = Z;
        }
    }

    public class Vec4
    {
        float v, x, y, z;

        Vec4(float V, float X, float Y, float Z)
        {
            v = V;
            x = X;
            y = Y;
            z = Z;
        }
    }

    public class Mat4
    {
        Vec3 element1, element2, element3, element4;

        Mat4(Vec3 Element1, Vec3 Element2, Vec3 Element3, Vec3 Element4)
        {
            element1 = Element1;
            element2 = Element2;
            element3 = Element3;
            element4 = Element4;
        }
    }

    public static class GeoObjectsgeo
    {

        public class ShadowInfo
        {
            Model shadow;
            Vec3 shadow_verts;
            int shadow_vert_count;
            Vec3 shadow_norms;
            int shadow_tris;
            int shadow_tri_count;
            int zero_area_tri_count;
            Vec3 tri_norms;
            byte open_edges; //AKA U8 in C
        }

        public class TexID
        {
            ushort id; //AKA U16 in C
            ushort count;
        }

        public class VertexBufferObject
        {
            int element_array_id;
            int tris;
            Vec2 verts;
            Vec3 norms;
            Vec2 sts;
            Vec2 sts2;
            Vec2 sts3;
            Vec4 tangents;
            Vec2 weights;
            Vec2 matidxs;
            ShadowInfo shadow;
            uint bump_init = 1; //AKA U32 in C
            uint in_use = 1;
            uint no_vbo = 1;
            uint sts2_init = 1;
            int tri_count, vert_count, vert_buffer_bytes, tex_count;
            TexID tex_ids;
            uint flags;
            int frame_id;
        }

        //Copied directly from master source
        public enum BoneID
        {
            BONEID_HIPS,
            BONEID_WAIST,
            BONEID_CHEST,
            BONEID_NECK,
            BONEID_HEAD,
            BONEID_COL_R,
            BONEID_COL_L,
            BONEID_UARMR,
            BONEID_UARML,
            BONEID_LARMR,
            BONEID_LARML,
            BONEID_HANDR,
            BONEID_HANDL,
            BONEID_F1_R,
            BONEID_F1_L,
            BONEID_F2_R,
            BONEID_F2_L,
            BONEID_T1_R,
            BONEID_T1_L,
            BONEID_T2_R,
            BONEID_T2_L,
            BONEID_T3_R,
            BONEID_T3_L,
            BONEID_ULEGR,
            BONEID_ULEGL,
            BONEID_LLEGR,
            BONEID_LLEGL,
            BONEID_FOOTR,
            BONEID_FOOTL,
            BONEID_TOER,
            BONEID_TOEL,

            BONEID_FACE,
            BONEID_DUMMY,
            BONEID_BREAST,
            BONEID_BELT,
            BONEID_GLOVEL,
            BONEID_GLOVER,
            BONEID_BOOTL,
            BONEID_BOOTR,
            BONEID_RINGL,
            BONEID_RINGR,
            BONEID_WEPL,
            BONEID_WEPR,        // I love how extents are L/R, but everything else is R/L
            BONEID_HAIR,
            BONEID_EYES,
            BONEID_EMBLEM,
            BONEID_SPADL,
            BONEID_SPADR,
            BONEID_BACK,
            BONEID_NECKLINE,
            BONEID_CLAWL,
            BONEID_CLAWR,
            BONEID_GUN,

            BONEID_RWING1,
            BONEID_RWING2,
            BONEID_RWING3,
            BONEID_RWING4,

            BONEID_LWING1,
            BONEID_LWING2,
            BONEID_LWING3,
            BONEID_LWING4,

            BONEID_MYSTIC,

            BONEID_SLEEVEL,
            BONEID_SLEEVER,
            BONEID_ROBE,
            BONEID_BENDMYSTIC,

            BONEID_COLLAR,
            BONEID_BROACH,

            BONEID_BOSOMR,
            BONEID_BOSOML,

            BONEID_TOP,            // really "shirt", but that's an alias for chest
            BONEID_SKIRT,
            BONEID_SLEEVES,

            BONEID_BROW,
            BONEID_CHEEKS,
            BONEID_CHIN,
            BONEID_CRANIUM,
            BONEID_JAW,
            BONEID_NOSE,

            BONEID_HIND_ULEGL,
            BONEID_HIND_LLEGL,
            BONEID_HIND_FOOTL,
            BONEID_HIND_TOEL,
            BONEID_HIND_ULEGR,
            BONEID_HIND_LLEGR,
            BONEID_HIND_FOOTR,
            BONEID_HIND_TOER,
            BONEID_FORE_ULEGL,
            BONEID_FORE_LLEGL,
            BONEID_FORE_FOOTL,
            BONEID_FORE_TOEL,
            BONEID_FORE_ULEGR,
            BONEID_FORE_LLEGR,
            BONEID_FORE_FOOTR,
            BONEID_FORE_TOER,

            BONEID_LEG_L_JET1,
            BONEID_LEG_L_JET2,
            BONEID_LEG_R_JET1,
            BONEID_LEG_R_JET2,

            // these are last!
            BONEID_COUNT,
            BONEID_INVALID = -1,
            BONEID_DEFAULT = 0
        }

        public class BoneInfo
        {
            int numbones;
            BoneID[] bone_id = new BoneID[15];
            Vec2 weights; //Same as in VBO
            short[] matidxs = new short[2]; //Same as in VBO
        }

        //Copied directly from master source
        public enum ModelFlags
        {
            OBJ_ALPHASORT = 1 << 0, // At least some of the subs on this model need to be alpha sorted
            OBJ_FANCYWATER = 1 << 1,
            OBJ_FULLBRIGHT = 1 << 2,
            OBJ_SUNFLARE = 1 << 3,
            OBJ_NOLIGHTANGLE = 1 << 4,


            OBJ_LOD = 1 << 7,
            OBJ_TREEDRAW = 1 << 8,
            OBJ_ALLOW_MULTISPLIT = 1 << 9,
            OBJ_FORCEOPAQUE = 1 << 10,
            OBJ_BUMPMAP = 1 << 11,
            OBJ_WORLDFX = 1 << 12, //draw this object using model draw
            OBJ_CUBEMAP = 1 << 13,
            OBJ_DRAWBONED = 1 << 14, //draw this object using model draw boned node
            OBJ_STATICFX = 1 << 15, //add static lighting to this object
            OBJ_HIDE = 1 << 16, //don't draw me except in wireframe

            OBJ_ALPHASORTALL = 1 << 18, // All subs on this model must be alpha sorted

            OBJ_DONTCASTSHADOWMAP = 1 << 19,            // will not appear in the shadow map
            OBJ_DONTRECEIVESHADOWMAP = 1 << 20,        // will not receive shadows from the shadow map
        }

        public class BoneAnimTrack
        {
            object rot_idx, pos_idx;
            ushort rot_fullkeycount, pos_fullkeycount, rot_count, pos_count;
            char id, flags;
            ushort pack_pad;
        }

        public class StAnim
        {
            BoneAnimTrack btTex0;
            BoneAnimTrack btText1;
            char name;
            float speed_scale;
            float st_scale;
            int flags;
        }

        public class AutoLOD
        {
            float max_error, lod_near, lod_far, lod_nearfade, lod_farfade;
            int flags;
            uint modelname_specified = 1;
            char lod_modelname, lod_filename;
        }

        public class TrickInfo
        {
            char file_name;
            char name;
            TrickNode tnode;
            ModelFlags model_flags;
            uint group_flags;
            float lod_near, lod_far, lod_nearfade, lod_farfade;
            StAnim stAnima;
            float shadow_dist, alpha_ref, alpha_ref_parser, tex_bias;
            Vec2 nightglow_times;
            Vec2 sway;
            Vec2 sway_random;
            Vec2 sway_pitch;
            Vec2 sway_roll;
            float lod_scale, tighten_up;
            uint fileAge;
            Vec3[] texScrollAmt;
            //this one in particular is an array of "VEC3", containing two elements;
            float alpha_sort_mod, water_reflection_skew, water_reflection_strength;
            AutoLOD auto_lod;
        }

        public class TrickNode
        {
            float tex_scale_y;
            float st_anim_age;
            byte[] trick_rgba = new byte[4];
            byte[] trick_rgba2 = new byte[4];
            int flags1;
            int flags2;
            ulong flags64; //Same as U64 in C
            TrickInfo info;
        }

        public enum TexUsage
        {
            TEX_FOR_UI = 1 << 0,
            TEX_FOR_FX = 1 << 1,
            TEX_FOR_ENTITY = 1 << 2,
            TEX_FOR_WORLD = 1 << 3,
            TEX_FOR_UTIL = 1 << 4,
            TEX_FOR_NOTSURE = 1 << 5
        }

        public enum TexFlags
        {
            TEX_ALPHA = 1 << 0,
            TEX_RGB8 = 1 << 1,
            TEX_COMP4 = 1 << 2,
            TEX_COMP8 = 1 << 3,

            TEX_TGA = 1 << 5,
            TEX_DDS = 1 << 6,


            TEX_CUBEMAPFACE = 1 << 9,
            TEX_REPLACEABLE = 1 << 10,
            TEX_BUMPMAP = 1 << 11,

            TEX_JPEG = 1 << 13
        }

        public enum TexOptFlags
        {
            TEXOPT_FADE = 1 << 0,
            TEXOPT_TRUECOLOR = 1 << 1,
            TEXOPT_TREAT_AS_MULTITEX = 1 << 2, // New texture, possibly rendered with old shader, but treat it as a new one
            TEXOPT_MULTITEX = 1 << 3, // New texture TEXLAYERing scheme
            TEXOPT_NORANDOMADDGLOW = 1 << 4,
            TEXOPT_FULLBRIGHT = 1 << 5,
            TEXOPT_CLAMPS = 1 << 6,
            TEXOPT_CLAMPT = 1 << 7,
            TEXOPT_ALWAYSADDGLOW = 1 << 8,
            TEXOPT_MIRRORS = 1 << 9,
            TEXOPT_MIRRORT = 1 << 10,
            TEXOPT_REPLACEABLE = 1 << 11,
            TEXOPT_BUMPMAP = 1 << 12,
            TEXOPT_REPEATS = 1 << 13,
            TEXOPT_REPEATT = 1 << 14,
            TEXOPT_CUBEMAP = 1 << 15,
            TEXOPT_NOMIP = 1 << 16,
            TEXOPT_JPEG = 1 << 17,
            TEXOPT_NODITHER = 1 << 18,
            TEXOPT_NOCOLL = 1 << 19,
            TEXOPT_SURFACESLICK = 1 << 20,
            TEXOPT_SURFACEICY = 1 << 21,
            TEXOPT_SURFACEBOUNCY = 1 << 22,
            TEXOPT_BORDER = 1 << 23,
            TEXOPT_OLDTINT = 1 << 24,
            TEXOPT_DOUBLEFUSION = 1 << 25,  // deprecated
            TEXOPT_POINTSAMPLE = 1 << 26,
            TEXOPT_NORMALMAP = 1 << 27,
            TEXOPT_SPECINALPHA = 1 << 28,
            TEXOPT_FALLBACKFORCEOPAQUE = 1 << 29,
        }

        public enum TexLoadState
        {
            TEX_NOT_LOADED = 1 << 0,
            TEX_LOADING = 1 << 1,
            TEX_LOADED = 1 << 2
        }

        public class TexReadInfo
        {
            byte data;
            int mip_count, format, width, height, size;
        }

        public enum TexWordLayerType
        {
            TWLT_NONE,
            TWLT_BASEIMAGE,
            TWLT_TEXT,
            TWLT_IMAGE
        }

        public enum TexWordLayerStretch
        {
            TWLS_NONE,
            TWLS_FULL,
            TWLS_TILE,
        }

        public class TexWordLayerFont
        {
            char fontName;
            int drawSize;
            bool italicize, bold;
            byte outlineWidth, dropShadowXOffset, dropShadowYOffset, softShadowSpread;
        }

        public enum TexWordBlendType
        {
            TWBLEND_OVERLAY,
            TWBLEND_MULTIPLY,
            TWBLEND_ADD,
            TWBLEND_SUBTRACT,
            TWBLEND_REPLACE
        }

        public class TexWordLayerFilter
        {
            TexWordLayerType type;
            int magnitude;
            float percent;
            int rgba;
            int[] offset = new int[2];
            float spread;
            TexWordBlendType blend;
        }

        public class TexWordLayer
        {
            char layerName;
            TexWordLayerType type;
            TexWordLayerStretch stretch;
            char text;
            Vec2 pos;
            Vec2 size;
            float rot;
            int rgba;
            int[] rgbas = new int[4];
            char imageName;
            bool hidden;
            TexWordLayerFont font;
            TexWordLayerFilter filter;

            TexWordLayer subLayer;
            TexWordBlendType subBlend;
            float subBlendWeight;

            BasicTexture image;
        }

        public class TexWord
        {
            char name;
            int width, height;
            TexWordLayer layers;
        }

        public class TexWordParams
        {
            char layoutName;
            char parameters;
        }

        public class TexWordLoadInfo
        {
            byte data;
            int actualSizeX, actualSizeY, sizeX, sizeY;
        }

        //Mostly ripped straight from master source and modified to convert from C to C#
        public class BasicTexture
        {
            char name;

            BasicTexture actualTexture; // This may be set to something different if the scene has swapped textures

            int width;        // Logical width/height
            int height;
            int id;            // OpenGL ID

            TexFlags flags;                //texture flags described below (TEX_ALPHA,TEX_RGB8, etc) (some set during texLoadData)

            TexOptFlags texopt_flags;
            uint texopt_surface;

            uint[] last_used_time_stamp = new uint[2] ;//0==regular, 1==raw
            TexLoadState[] load_state = new TexLoadState[2];        //has the data been loaded (are the id handles valid for it) 0==regular, 1==raw

            uint file_pos;                //Offset of the texture data from file start
            uint file_bytes;                //Size of texture data
            char dirname;                //texture directory name ("WORLD/Filler/") - tack on texture_library/ and name and .texture to get the filename

            int target;                    //OpenGL target flag (GL_TEXTURE_CUBE_MAP_ARB or GL_TEXTURE_2D)
            float gloss;                    //for bumpmapping

            // Preloaded mipmap data
            char mipdata;
            int mipsize;
            int mipid;

            TexReadInfo rawInfo;
            int rawReferenceCount;        // Count of texLoadRawData()s outstanding

            // Texture compositing
            TexWord texWord;
            int origWidth, origHeight;
            TexWordParams texWordParams; // For dynamic textures
            TexWordLoadInfo texWordLoadInfo;

            uint hasBeenComposited = 1;    // If the texture has a texWord*, has it been applied?

            TexUsage use_category;    //what is this texture used for?
            int[] memory_use = new int[2];

            short cube_face_idx = 8;        // Used by textures that are cube maps 

            int realWidth, realHeight;    // GL size of the texture, power of 2

        }

        public enum TexOptScrollType
        {
            TEXOPTSCROLL_NORMAL,
            TEXOPTSCROLL_PINGPONG,
            TEXOPTSCROLL_OVAL,
        }

        public class ScrollsScales
        {
            float[][] texopt_scale = new float[13][] { new float[2], new float[2], new float[2], new float[2], new float[2], new float[2],
                new float[2], new float[2], new float[2], new float[2], new float[2], new float[2], new float[2]};
            float[][] texopt_scroll = new float[13][] { new float[2], new float[2], new float[2], new float[2], new float[2], new float[2],
                new float[2], new float[2], new float[2], new float[2], new float[2], new float[2], new float[2]};
            TexOptScrollType[] texopt_scrollType = new TexOptScrollType[13] {new TexOptScrollType(), new TexOptScrollType(), new TexOptScrollType(),
                new TexOptScrollType(), new TexOptScrollType(), new TexOptScrollType(), new TexOptScrollType(), new TexOptScrollType(),
                new TexOptScrollType(), new TexOptScrollType(), new TexOptScrollType(), new TexOptScrollType(), new TexOptScrollType() };
            byte alphaMask, hasRgb34, multiply1Reflect, multiply2Reflect, baseAddGlow, minAddGlow, maxAddGlow, addGlowMat2, tintGlow, tintReflection,
                desaturateReflection, alphaWater;
            byte[] rgba3 = new byte[4];
            byte[] rgba4 = new byte[4];
            byte[] specularRgba1 = new byte[4];
            byte[] specularRgba2 = new byte[4];
            float specularExponent1, specularExponent2, reflectivity, reflectivityBase, reflectivityScale, reflectivityPower, maskWeight;
            float[] diffuseScale = new float[3];
            float[] ambientScale = new float[3];
            float[] diffuseScaleTrick = new float[3];
            float[] ambientScaleTrick = new float[3];
            float[] ambientMin = new float[3];
            TexOpt debug_backpointer;
        }

        public enum BlendModeShader
        {
            BLENDMODE_MODULATE,
            BLENDMODE_MULTIPLY,
            BLENDMODE_COLORBLEND_DUAL,
            BLENDMODE_ADDGLOW,            // building windows, neon, etc
            BLENDMODE_ALPHADETAIL,
            BLENDMODE_BUMPMAP_MULTIPLY,
            BLENDMODE_BUMPMAP_COLORBLEND_DUAL,
            BLENDMODE_WATER,
            BLENDMODE_MULTI,
            BLENDMODE_SUNFLARE,
            BLENDMODE_NUMENTRIES
        }

        public class TexOptFallback
        {
            char base_name;
            char blend_name;
            char bumpmap;
            BlendModeShader blend_mode;
            Vec2[] scaleST = new Vec2[2];
            Vec3 diffuseScaleTrick;
            Vec3 ambientScaleTrick;
            Vec3 ambientMin;
            byte useFallback;
        }

        public class TexOpt
        {
            char file_name;
            char name;
            char[] blend_names = new char[13];
            byte[] swappable = new byte[13];
            float[] texopt_fade = new float[2];
            ScrollsScales scrollsScales;
            TexOptFlags flags;
            BlendModeShader blend_mode;
            int surface;
            char surface_name, df_name;
            float gloss, specularExponent1_parser, specularExponent2_parser;
            uint fileAge;
            ModelFlags model_flags;
            TexOptFallback fallback;
        }

        public class TexBind
        {
            char name;
            int width;
            int height;
            string dirname;
            BlendModeType bind_blend_mode;
            float[][] bind_scale = new float[2][] { new float[2], new float[2] }; //Another Jagged Array, yay!
            TexUsage use_category;
            BasicTexture[] tex_layers = new BasicTexture[11];
            byte[] tex_swappable = new byte[11];
            byte needs_alphasort = 1;
            byte allocated_scrollsScales = 1;
            byte allocated_byMiniTracker = 1;
            byte is_fallback_material = 1;
            TexOpt texopt;
            ScrollsScales scrollsScales;
            TexBind tex_lod;
            TexBind orig_bind;
        }

        public class ModelLODInfo
        {
            byte is_automatic = 1;
            byte is_from_trick = 1;
            byte is_from_default = 1;
            byte is_no_lod = 1;
            uint has_bits;
            bool force_auto;
            bool removed;
            char modelname;
            char parsed_filename;
            AutoLOD lods;
        }

        public class PolyCell
        {
            PolyCell children;
            ushort tri_idxs;
            int tri_count;
        }

        public class PolyGrid
        {
            PolyCell cell;
            float[] pos = new float[3];
            float size, inv_size;
            int tag, num_bits;
        }

        public class CTri
        {
            float[] V1 = new float[3];
            float[] V2 = new float[2];
            float[] V3 = new float[2];
            float[] midpt = new float[2];
            float[] extents = new float[2];
            float[] norm = new float[3];
            float scale;
            uint flags;
            BasicTexture surfText;
        }

        public class GMeshReductions
        {
            int num_reductions, num_tris_left;
            float error_values;
            int remaps_counts, changes_counts, total_remaps, remaps, total_remap_tris, remap_tris, total_changes, changes;
            float positions, tex1s;
        }

        public class LodModel
        {
            float tri_percent, error;
            Model model;
        }



        public class AltPivotInfo
        {
            int altpivotcount;
            Mat4[] altpivot = new Mat4[15];
        }

        public class Portal
        {
            Vec3 pos, min, max, normal;
        }

        public class ModelExtra
        {
            int portal_count;
            Portal[] portals = new Portal[8];
        }

        public class PackData
        {
            int packSize;
            uint unpacksize;
            byte data;
        }

        public class SharedHeapHandle
        {
            object data;
            char[] pcName = new char[256];
            ulong uiRefCount;
            ulong uiSize;
        }
        public class BlendModeType
        {
            uint inval;
            short shader;
            ushort blend_bits;
        }

        public class Model
        {
            public uint flags;
            public uint radius;
            VertexBufferObject vbo;
            short id;  //AKA S16 in C
            byte loadstate;
            BoneInfo boneInfo;
            TrickNode trick;
            int vert_count, tri_count, reflection_quad_count;
            GeoLoadData gld;
            int tex_count;
            TexID tex_idx;
            BlendModeType common_blend_mode;
            BlendModeType blend_modes;
            TexBind tex_binds;
            ModelLODInfo lod_info;
            PolyGrid grid;
            CTri ctris;
            int ctriflags_setonall;
            int ctriflags_setonsome;
            int tags;
            float grid_size_orig;

            struct unpack
            {
                int tris;
                Vec3 verts, norms;
                Vec2 sts, sts3;
                GMeshReductions reductions;
                Vec3 reflection_quads;
            }

            Model srcmodel;
            LodModel lod_models;
            float[] autolod_dists = new float[3];
            float lod_minerror, lod_maxerror, lod_mintripercent, lod_maxtripercent;

            char name;
            int namelen, namelen_notrick;
            char filename;
            AltPivotInfo api;
            ModelExtra extra;
            Vec3 scale, min, max;

            struct pack { PackData tris, verts, norms, sts, sts3, weights, matidxs, grid, reductions, reflection_quads; }
        }

        public class ModelHeader
        {
            string name;
            Model model_data;
            float length;
            Model models;
            int model_count;
        }

        public class PackNames
        {
            char strings;
            int count;
        }

        public enum TexLoadHow {
            TEX_LOAD_IN_BACKGROUND = 1,
            TEX_LOAD_NOW_CALLED_FROM_MAIN_THREAD,
            TEX_LOAD_NOW_CALLED_FROM_LOAD_THREAD,
            TEX_LOAD_DONT_ACTUALLY_LOAD,
            TEX_LOAD_IN_BACKGROUND_FROM_BACKGROUND,
        };

        public enum GeoUseType
        {
            GEO_DONT_INIT_FOR_DRAWING = 1 << 0, //just called from geoload
            GEO_INIT_FOR_DRAWING = 1 << 1, //called from modelload
            GEO_USED_BY_WORLD = 1 << 2,
            GEO_USED_BY_GFXTREE = 1 << 3,
            GEO_GETVRML_FASTLOAD = 1 << 4,
            GEO_USE_MASK = GEO_USED_BY_WORLD | GEO_USED_BY_GFXTREE,
        };

        public class GeoLoadData
        {
            public GeoLoadData next;
            public GeoLoadData prev;
            public ModelHeader modelheader;
            public string name;
            public PackNames texnames;
            public int headersize, datasize, loadstate;
            public float lasttimeused;
            public int type;
            public object file;
            public TexLoadHow tex_load_style;
            public GeoUseType geo_use_type;
            public object header_data;
            public object geo_data;
            public int data_offset;
            public int file_format_version;
            public ModelLODInfo lod_infos;
        }

        public static GeoLoadData GetGeoLoadData(string FileName)
        {
            try
            {
                GeoLoadData gld = new GeoLoadData();

                //Not sure what this is, but master code is doing this, so I do it
                int ziplen, oziplen;
                using (BinaryReader br = new BinaryReader(new FileStream(FileName, FileMode.Open)))
                {
                    ziplen = (int)br.ReadUInt32();
                    oziplen = ziplen;
                    gld.headersize = (int)br.ReadUInt32();
                    int versionNumber = 0;
                    if (gld.headersize == 0)
                    {
                        // new format
                        versionNumber = (int)br.ReadUInt32();
                        if (versionNumber >= 2 && versionNumber <= 8 && versionNumber != 6)
                        {
                            ziplen -= 0;
                        }
                        else throw new Exception("Bad version number '" + versionNumber + "'");

                        ziplen -= 12; // was biased to include 2 * sizeof(gld->headersize) + sizeof(version_num) so pigg system would cache it
                        gld.headersize = (int)br.ReadUInt32();
                    }
                    else
                    {
                        // old format
                        ziplen -= 4; // was biased to include sizeof(gld->headersize) so pigg system would cache it
                        oziplen += 4; // because of an old bug in getVRML
                    }

                    gld.file_format_version = versionNumber;
                    byte[] zipmem = br.ReadBytes(ziplen);
                    zlib.ZInputStream zIn = null;
                    byte[] zOut = null;
                    using (zIn = new zlib.ZInputStream(new MemoryStream(zipmem)))
                    {
                        zOut = zIn.ReadBytes(ziplen);
                    }

                }

                return gld;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to load GeoLoadData object.", ex);
            }
        }
    }

}
